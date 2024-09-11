module LibClient.Components.InfoMessageStyles

open ReactXP.LegacyStyles
type Level = LibClient.Components.InfoMessage.Level

let private baseStyles = lazy (asBlocks [
    "view" => [
        marginVertical 16
        // the parent component may not have AlignItems.Stretch,
        // so the whole unit may collapse to just this line, in which
        // case TextAlign.Center will be meaningless, so we should
        // at least give it some left padding (but really on both
        // sides, to keep it balanced)
        paddingHorizontal 20
    ]

    "text" => [
        TextAlign.Center
    ]
])

type LevelColors = {
    Info:      Color
    Attention: Color
    Caution:   Color
}

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.InfoMessage", baseStyles)

    static member All (levelColors: LevelColors) : unit =
        customize [
            Theme.Rules (levelColors)
        ]

    static member One (levelColors: LevelColors) : Styles =
        Theme.Rules (levelColors) |> makeSheet


    static member Rules (levelColors: LevelColors) : List<ISheetBuildingBlock> = [
        "text" => [] && [
            Level.Info.ToString() => [
                color levelColors.Info
            ]

            Level.Attention.ToString() => [
                color levelColors.Attention
            ]

            Level.Caution.ToString() => [
                color levelColors.Caution
            ]
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules ({
        Info      = Color.Grey "66"
        Attention = Color.DevOrange
        Caution   = Color.DevRed
    })
]))
