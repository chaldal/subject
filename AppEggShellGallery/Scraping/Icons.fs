module Scraping.Icons

open LibClient.Icons

let private nonIconMethodNames: Set<string> = set [
    "Equals"
    "GetHashCode"
    "ToString"
    "GetType"
    "MakeSvgPathIcon"
    "MakeIcon"
]

let icons : string =
    let nameToConstructorPairs : List<string> =
        let allMethodNames =
            typeof<Icon>.GetMethods()
            |> Seq.map (fun method -> method.Name)
            |> Set.ofSeq

        Set.difference allMethodNames nonIconMethodNames
        |> Set.map (fun methodName -> methodName.Substring("get_".Length))
        |> Set.toList
        |> List.sort
        |> List.map (fun iconName -> sprintf """    ("%s", Icon.%s)""" iconName iconName)

    sprintf "[\n%s\n  ]" (String.concat "\n" nameToConstructorPairs)
