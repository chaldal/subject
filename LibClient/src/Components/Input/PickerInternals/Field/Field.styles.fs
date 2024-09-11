module LibClient.Components.Input.PickerInternals.FieldStyles

open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let tapCapture =
        makeViewStyles {
            opacity 0.
        }

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "view" => [
        paddingTop 6 // to make space for the label
    ]

    "handheld-full-width-tap-area" => [
        flex 1
        AlignSelf.Stretch
    ]

    "border" => [
        borderWidth       1
        borderRadius      4
        paddingHorizontal 10
        backgroundColor   Color.White
        AlignItems.Center
        FlexDirection.RowReverse
        JustifyContent.SpaceBetween
    ]

    "picker-values" => [
        FlexDirection.Row
        flexGrow   0
        flexShrink 1
    ]

    "picker-actions" => [
        FlexDirection.Row
    ]

    "tag" => [
        marginRight     4
        borderRadius    3
        border          1 (Color.Grey "e8")
        backgroundColor (Color.Grey "e8")
        FlexDirection.Row
    ] && [
        "highlighted" => [
            borderColor  Color.DevRed
        ]
    ]

    "tag-text" => [
       paddingHorizontal 6
    ]

    "text-input" => [
        flex 1
    ]

    "invalid-reason" => [
        fontSize 12
    ]

    "label" => [
        Position.Absolute
        top               0
        left              10
        paddingHorizontal 3
        backgroundColor   Color.White
    ]

    "label-text" => [
        fontSize 12
        FontWeight.W700
    ]
])

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Input.PickerInternals.Field", baseStyles)

    static member All (borderLabelColor: Color, borderLabelFocusColor: Color, borderLabelInvalidColor: Color, textColor: Color, invalidReasonColor: Color, placeholderColor: Color, theVerticalPadding: int, iconSize: int) : unit =
        Theme.Customize [
            Theme.Rules(borderLabelColor, borderLabelFocusColor, borderLabelInvalidColor, textColor, invalidReasonColor, placeholderColor, theVerticalPadding, iconSize)
        ]

    // NOTE this doesn't actually work right now, because we dont' have a way of passing a theme
    // down the component tree, which would be required for Picker > Base > Field
    static member One (borderLabelColor: Color, borderLabelFocusColor: Color, borderLabelInvalidColor: Color, textColor: Color, invalidReasonColor: Color, placeholderColor: Color, theVerticalPadding: int, iconSize: int) : Styles =
        Theme.Rules(borderLabelColor, borderLabelFocusColor, borderLabelInvalidColor, textColor, invalidReasonColor, placeholderColor, theVerticalPadding, iconSize) |> makeSheet

    static member Rules (borderLabelColor: Color, borderLabelFocusColor: Color, borderLabelInvalidColor: Color, textColor: Color, invalidReasonColor: Color, placeholderColor: Color, theVerticalPadding: int, iconSize: int) : List<ISheetBuildingBlock> = [
        "selected-item" => [
           flex 1
           color textColor
        ]

        "icon" => [
            fontSize iconSize
            color    textColor
        ]

        "invalid-reason" => [
            color invalidReasonColor
        ]

        "border" => [
            borderColor     borderLabelColor
            paddingVertical theVerticalPadding
        ] && [
            "border-focused" => [
                borderColor borderLabelFocusColor
            ]

            "border-invalid" => [
                borderColor borderLabelInvalidColor
            ]
        ]

        "label-text" => [
            color borderLabelColor
        ] && [
            "focused" => [
                color borderLabelFocusColor
            ]

            "invalid" => [
                color borderLabelInvalidColor
            ]
        ]

        "SPECIAL-placeholder" => [
            color placeholderColor
        ]
    ]

    static member Border ( rules: seq<RuleFunctionReturnedStyleRules> ) : List<ISheetBuildingBlock> = [
        "border" => rules
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        borderLabelColor        = Color.Grey "44",
        borderLabelFocusColor   = Color.DevGreen,
        borderLabelInvalidColor = Color.DevRed,
        textColor               = Color.Grey "44",
        invalidReasonColor      = Color.DevRed,
        placeholderColor        = Color.Grey "aa",
        iconSize                = 20,
        theVerticalPadding      = 10
    )
]))
