module LibClient.Components.Input.CheckboxStyles

open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let tapCapture =
        makeViewStyles {
            trbl -10 -10 -10 -10
        }

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "top-level-div" => [
        Overflow.VisibleForTapCapture
    ]

    "div" => [
        FlexDirection.Row
        AlignItems.Center
        flex 1
        Overflow.VisibleForTapCapture
    ]

    "label" => [
        flex        1
        paddingLeft 12
    ]

    "invalid-reason" => [
        color Color.DevRed
    ]

    "icon && checkbox-invalid" => [
        color Color.DevRed
    ]
])

let private defaultIconSize = 20

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Input.Checkbox", baseStyles)

    static member All (iconCheckedColor: Color, iconUncheckedColor: Color, labelColor: Color, ?maybeIconSize: int) : unit =
        let iconSize = defaultArg maybeIconSize defaultIconSize

        customize [
            Theme.Rules(iconCheckedColor, iconUncheckedColor, labelColor, iconSize)
        ]

    static member One (iconCheckedColor: Color, iconUncheckedColor: Color, labelColor: Color, ?maybeIconSize: int) : Styles =
        let iconSize = defaultArg maybeIconSize defaultIconSize

        Theme.Rules(iconCheckedColor, iconUncheckedColor, labelColor, iconSize) |> makeSheet


    static member Rules (iconCheckedColor: Color, iconUncheckedColor: Color, labelColor: Color, iconSize: int) : List<ISheetBuildingBlock> = [
        "icon && checked" => [
            color iconCheckedColor
        ]

        "icon && unchecked" => [
            color iconUncheckedColor
        ]

        "label" => [
            color labelColor
        ]

        "icon" => [
            fontSize iconSize
        ]
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        iconCheckedColor   = Color.Grey "aa",
        iconUncheckedColor = Color.Grey "aa",
        labelColor         = Color.Black,
        iconSize           = defaultIconSize
    )
]))
