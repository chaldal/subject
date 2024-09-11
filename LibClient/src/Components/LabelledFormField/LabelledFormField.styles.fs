module LibClient.Components.LabelledFormFieldStyles

open LibClient.Responsive
open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "view" => [
        Overflow.Visible
    ]

    "field" => [
        Overflow.Visible
    ]

    ScreenSize.Desktop.Class => [] && [
        "view" => [
            FlexDirection.Row
            AlignItems.Center
            JustifyContent.FlexEnd
            padding 10
        ]

        "label" => [
            width 100
            flex  0
            color (Color.Grey "66")
        ]

        "field" => [
            flex 1
        ]
    ]

    ScreenSize.Handheld.Class => [] && [
        "view" => [
            FlexDirection.Column
            marginBottom 12
        ]

        "label" => [
            marginBottom 6
            fontSize     14
            color        (Color.Grey "66")
        ]
    ]
])

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.LabelledFormField", baseStyles)

    static member LabelWidth (value: int) : Styles =
        Theme.LabelWidthBlocks value |> makeSheet

    static member LabelWidthBlocks (value: int) : List<ISheetBuildingBlock> =
        [
            "label" => [] && [
                ScreenSize.Desktop.Class => [
                    width value
                ]
            ]
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.LabelWidthBlocks 100
]))
