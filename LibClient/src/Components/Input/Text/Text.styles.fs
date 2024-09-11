module LibClient.Components.Input.TextStyles

open LibClient.Input
open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "view" => [
        Overflow.Visible
    ]

    "withLabel" => [
       marginTop 6 // required to show the label when the input is focused
    ]

    "border" => [
        AlignItems.Center
        FlexDirection.Row
        borderWidth       1
        backgroundColor   Color.White
        paddingHorizontal 10
    ]

    "prefix" => [
        flex 0
        paddingTop 1 // for vertical alignment with the textbox
    ]

    "suffix-icon" => [
        fontSize 20
    ]

    "focus-preserving-sentinel" => [
        width  0
        height 0
    ]

    "text-input" => [
        flex   1
    ]

    "single-line" => [
        height 21
    ]

    "text-input-noneditable" => [
        flex 1
    ]

    "invalid-reason" => [
        fontSize 12
    ]

    "label" => [
        Position.Absolute
        top               13
        left              10
        paddingHorizontal 3
        backgroundColor   Color.White
    ] && [
        "small" => [
            // NOTE we can't do this with transforms, because
            // ReactXP (because of ReactNative) does not support
            // transform-origin, so there's no way to set the transformation
            // origin to (0, 0), unless we have support for getting the
            // components' dimensions at runtime, which we currently don't.
            top  -6
            left 10
        ]
    ]

    "label-text" => [
        fontSize 16
    ] && [
        "small" => [
            fontSize 12
            FontWeight.W700
        ]
    ]
])

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Input.Text", baseStyles)

    static member All (borderLabelBlurredColor: Color, borderLabelFocusedColor: Color, borderLabelInvalidColor: Color, textColor: Color, noneditableTextColor: Color, noneditableBackgroundColor: Color, invalidReasonColor: Color, placeholderColor: Color, theVerticalPadding: int, ?theBorderRadius: int) : unit =
        Theme.Customize [
            Theme.Rules(borderLabelBlurredColor, borderLabelFocusedColor, borderLabelInvalidColor, textColor, noneditableTextColor, noneditableBackgroundColor, invalidReasonColor, placeholderColor, theVerticalPadding, ?theBorderRadius = theBorderRadius)
        ]

    static member One (borderLabelBlurredColor: Color, borderLabelFocusedColor: Color, borderLabelInvalidColor: Color, textColor: Color, noneditableTextColor: Color, noneditableBackgroundColor: Color, invalidReasonColor: Color, placeholderColor: Color, theVerticalPadding: int, ?theBorderRadius: int) : Styles =
        Theme.Rules(borderLabelBlurredColor, borderLabelFocusedColor, borderLabelInvalidColor, textColor, noneditableTextColor, noneditableBackgroundColor, invalidReasonColor, placeholderColor, theVerticalPadding, ?theBorderRadius = theBorderRadius) |> makeSheet

    // For controlling the min-height of multiline input
    static member MinHeight (theHeight: int) : Styles =
        makeSheet [
            "text-input" => [
                minHeight theHeight
            ]
        ]

    static member ZeroPadding (): Styles =
        makeSheet [
            "border" => [
                paddingVertical   0
                paddingHorizontal 0
            ]
        ]

    static member FontFamily(theFontFamily: string): List<ISheetBuildingBlock> =
        [
            "defaults" => [
                fontFamily theFontFamily
            ]
        ]

    static member Rules (
        borderLabelBlurredColor: Color,
        borderLabelFocusedColor: Color,
        borderLabelInvalidColor: Color,
        textColor: Color,
        noneditableTextColor: Color,
        noneditableBackgroundColor: Color,
        invalidReasonColor: Color,
        placeholderColor: Color,
        theVerticalPadding: int,
        ?theBorderRadius: int
    ) : List<ISheetBuildingBlock> =
        let theBorderRadius = defaultArg theBorderRadius 4

        [
            "border" => [
                borderRadius    theBorderRadius
                borderColor     borderLabelBlurredColor
                paddingVertical theVerticalPadding
            ] && [
                "border-focused" => [
                    borderColor borderLabelFocusedColor
                ]
                "border-invalid" => [
                    borderColor borderLabelInvalidColor
                ]
                "border-noneditable" => [
                    backgroundColor noneditableBackgroundColor
                ]
            ]

            "suffix-icon" => [
                color textColor
            ]

            "suffix-text" => [
                color textColor
            ]

            "invalid-reason" => [
                color invalidReasonColor
            ]

            "label-text" => [
                color borderLabelBlurredColor
            ] && [
                "focused" => [
                    color borderLabelFocusedColor
                ]
                "invalid" => [
                    color borderLabelInvalidColor
                ]
            ]

            "text-input" => [
                color textColor
            ] && [
                "noneditable" => [
                    color noneditableTextColor
                    backgroundColor noneditableBackgroundColor
                ]
            ]

            "prefix" => [
                color textColor
            ]

            "SPECIAL-placeholder" => [
                color placeholderColor
            ]
        ]

    static member Border ( rules: seq<RuleFunctionReturnedStyleRules> ) : List<ISheetBuildingBlock> = [
            "border" => rules
        ]

    static member CustomStyles ( rules: seq<RuleFunctionReturnedStyleRules> ) =
        let customStyle : List<List<ISheetBuildingBlock>> = [[
            "text-input" => rules
        ]]

        List.concat (baseStyles.Value :: customStyle) |> makeSheet

    static member TextInputCustomStyles ( rules: seq<RuleFunctionReturnedStyleRules> ) : List<ISheetBuildingBlock> = [
            "text-input" => rules
        ]

    static member LabelStyles ( rules: seq<RuleFunctionReturnedStyleRules> ) : List<ISheetBuildingBlock> = [
            "label" => rules
        ]

    static member LabelTextStyles ( rules: seq<RuleFunctionReturnedStyleRules> ) : List<ISheetBuildingBlock> = [
            "label-text" => rules
        ]

    static member InvalidReasonStyles ( rules: seq<RuleFunctionReturnedStyleRules> ) : List<ISheetBuildingBlock> = [
            "invalid-reason" => rules
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        borderLabelBlurredColor    = Color.Grey "44",
        borderLabelFocusedColor    = Color.DevGreen,
        borderLabelInvalidColor    = Color.DevRed,
        textColor                  = Color.Grey "44",
        noneditableTextColor       = Color.Grey "22",
        noneditableBackgroundColor = Color.Grey "66",
        invalidReasonColor         = Color.DevRed,
        placeholderColor           = Color.Grey "aa",
        theVerticalPadding         = 10
    )
]))
