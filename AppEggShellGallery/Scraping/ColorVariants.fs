module Scraping.ColorVariants

open LibClient.ColorScheme

let colorVariants : string =
    let materialColors : seq<string> =
        typeof<MaterialDesignColors.Marker>.DeclaringType.GetProperties()
            |> Seq.map (fun property -> property.Name)

    sprintf "[\n%s\n  ]"
        (materialColors
        |> Seq.map (fun color ->
            sprintf """    ("%s", MaterialDesignColors.``%s``)""" color color
        )
        |> String.concat "\n")
