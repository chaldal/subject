module LibClient.Components.Sidebar.HeadingStyles

open ReactXP.LegacyStyles

let baseStyles = lazy (asBlocks [
    "view" => [
        marginHorizontal 18
        marginVertical   18
    ]
])

type Heading.Level with
    member this.DefaultFontSize : int =
        match this with
        | Heading.Level.Primary   -> 18
        | Heading.Level.Secondary -> 14

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Sidebar.Heading", baseStyles)

    static member private Rules (textColor: Color, theFontSize: int) : List<RuleFunctionReturnedStyleRules> =
        [
            color    textColor
            fontSize theFontSize
        ]

    static member One (level: Heading.Level,textColor: Color, ?maybeFontSize: int) : Styles =
        let fontSize = defaultArg maybeFontSize level.DefaultFontSize

        Theme.Part(level, textColor, fontSize) |> makeSheet

    static member Part (level: Heading.Level,textColor: Color, ?maybeFontSize: int) : List<ISheetBuildingBlock> =
        let fontSize = defaultArg maybeFontSize level.DefaultFontSize

        [
            (sprintf "view && level-%O" level) => Theme.Rules (textColor, fontSize)
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Part(Heading.Level.Primary,   textColor = Color.DevBlue)
    Theme.Part(Heading.Level.Secondary, textColor = Color.DevBlue)
]))