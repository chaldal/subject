module LibClient.Components.Input.NamedFileStyles

open LibClient.Responsive
open ReactXP.Styles
module Styles =
    let selectFile =
        makeViewStyles {
            paddingHorizontal 60
        }

open ReactXP.LegacyStyles
let private baseStyles = lazy (asBlocks [
    "view" => [
        flex      1
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
    static let customize = makeCustomize("LibClient.Components.Input.NamedFile", baseStyles)

    static member All (invalidColor: Color, dropZoneBorderColor: Color) : unit =
        customize [
            Theme.Rules (invalidColor, dropZoneBorderColor)
        ]

    static member Rules (invalidColor: Color, dropZoneBorderColor: Color) : List<ISheetBuildingBlock> = [
        ScreenSize.Desktop.Class => [] && [
            "view" => [
                borderStyle BorderStyle.Dashed
                border       2 dropZoneBorderColor
                borderRadius 6
            ]
        ]

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
        invalidColor        = Color.DevRed,
        dropZoneBorderColor = Color.DevLightGrey
    )
]))