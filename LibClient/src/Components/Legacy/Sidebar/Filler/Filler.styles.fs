module LibClient.Components.Legacy.Sidebar.FillerStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "view" => [
        flex              1
        borderBottomWidth 1
        backgroundColor   (Color.Grey "f9")
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Legacy.Sidebar.Filler", baseStyles)

    static member All
        (
            bottomBorderColor: Color
        ) : unit =
        customize [
            Theme.Rules(
                bottomBorderColor
            )
        ]

    static member Rules
        (
            bottomBorderColor: Color
        ) : List<ISheetBuildingBlock> =
        [
            "view" => [
                borderColor bottomBorderColor
            ]
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        bottomBorderColor = Color.Black
    )
]))
