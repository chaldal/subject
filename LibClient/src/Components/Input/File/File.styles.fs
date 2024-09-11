module LibClient.Components.Input.FileStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "view" => [
        padding   5
        marginTop 10
        AlignItems.Center
    ]

    "text-center" => [
        TextAlign.Center
    ]

    "drag-and-drop-message" => [
        marginTop 20
    ]

    "invalid-reason" => [
        TextAlign.Center
    ]

    "info-message" => [
        color Color.DevOrange
    ]

    "message-container" => [
        marginTop 10
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Input.File", baseStyles)

    static member All (invalidColor: Color) : unit =
        customize [
            Theme.Rules(invalidColor)
        ]

    static member One (invalidColor: Color) : Styles =
        Theme.Rules(invalidColor) |> makeSheet

    static member Rules (invalidColor: Color) : List<ISheetBuildingBlock> = [
        "view && border-invalid" => [
            borderColor invalidColor
        ]

        // TODO create Input.InvalidMessage component with a theme
        "invalid-reason" => [
            color invalidColor
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        invalidColor = Color.DevRed
    )
]))