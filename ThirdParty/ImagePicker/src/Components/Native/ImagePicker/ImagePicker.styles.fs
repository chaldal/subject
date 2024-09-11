module ThirdParty.ImagePicker.Components.Native.ImagePickerStyles

open ReactXP.Styles

module Styles =
    let imageThumbs =
        makeViewStyles {
            AlignItems.Center
            JustifyContent.Center
        }

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

    "constrain-message" => [
        marginTop 10
    ]

    "invalid-reason" => [
        TextAlign.Center
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("ThirdParty.ImagePicker.Components.Native.ImagePicker", baseStyles)

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