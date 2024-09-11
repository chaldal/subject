module LibClient.Components.Input.ParsedTextStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Input.ParsedText", baseStyles)

    static member All (borderLabelBlurredColor: Color, borderLabelFocusedColor: Color, borderLabelInvalidColor: Color, textColor: Color, noneditableTextColor: Color, noneditableBackgroundColor: Color, invalidReasonColor: Color, placeholderColor: Color, theVerticalPadding: int) : unit =
        customize [
            Theme.Rules(borderLabelBlurredColor, borderLabelFocusedColor, borderLabelInvalidColor, textColor, noneditableTextColor, noneditableBackgroundColor, invalidReasonColor, placeholderColor, theVerticalPadding)
        ]

    static member One (borderLabelBlurredColor: Color, borderLabelFocusedColor: Color, borderLabelInvalidColor: Color, textColor: Color, noneditableTextColor: Color, noneditableBackgroundColor: Color, invalidReasonColor: Color, placeholderColor: Color, theVerticalPadding: int) : Styles =
        Theme.Rules(borderLabelBlurredColor, borderLabelFocusedColor, borderLabelInvalidColor, textColor, noneditableTextColor, noneditableBackgroundColor, invalidReasonColor, placeholderColor, theVerticalPadding) |> makeSheet

    // For controlling the min-height of multiline input
    static member MinHeight (theHeight: int) : Styles =
        makeSheet [
            "theme" ==> LibClient.Components.Input.TextStyles.Theme.MinHeight theHeight
        ]

    static member ZeroPadding (): Styles =
        makeSheet [
            "theme" ==> LibClient.Components.Input.TextStyles.Theme.ZeroPadding ()
        ]

    static member Rules (borderLabelBlurredColor: Color, borderLabelFocusedColor: Color, borderLabelInvalidColor: Color, textColor: Color, noneditableTextColor: Color, noneditableBackgroundColor: Color, invalidReasonColor: Color, placeholderColor: Color, theVerticalPadding: int) : List<ISheetBuildingBlock> = [
        "theme" ==> LibClient.Components.Input.TextStyles.Theme.One (borderLabelBlurredColor, borderLabelFocusedColor, borderLabelInvalidColor, textColor, noneditableTextColor, noneditableBackgroundColor, invalidReasonColor, placeholderColor, theVerticalPadding)
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
