module LibClient.Components.Legacy.Sidebar.ItemStyles

open ReactXP.LegacyStyles

// TODO deal with proper palette, either this one is really supposed to be custom,
// or I need to derive proper Neutral palette out of the colours used in the new mocks
let private baseStyles = lazy (asBlocks [
    "item" => [
        FlexDirection.Column
        paddingHorizontal 16
        Cursor.Pointer
    ]
    && [
        "Primary" => [
            height   52
        ]

        "Secondary" => [
            height   40
        ]
    ]

    "content" => [
        flex 1
        FlexDirection.Row
        AlignItems.Center
    ]

    "icon" => [
        marginBottom 1 // vertical alignment tweak
        flex         0
    ] && [
        "icon-left" => [
            marginRight 10
        ]
        "icon-right" => [
            marginLeft 10
        ]
    ]

    "text" => [
        flex 1
        FontWeight.W600
    ] && [
        "Primary" => [
            FontWeight.Normal
            fontSize 18
        ]

        "Secondary" => [
            FontWeight.Normal
            fontSize 16
        ]
    ]

    "count" => [
        FlexDirection.Row
        AlignItems.Center
        JustifyContent.Center
        Cursor.Pointer
        fontSize          16
        height            30
        minWidth          30
        paddingHorizontal 8
        flex              0
        borderRadius      15
    ]

    "count-text" => [
        FontWeight.W600
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Legacy.Sidebar.Item", baseStyles)

    static member All
        (
            primaryBackgroundColor:           Color,
            primaryTextColor:                 Color,
            primarySelectedBackgroundColor:   Color,
            primarySelectedTextColor:         Color,
            secondaryBackgroundColor:         Color,
            secondaryTextColor:               Color,
            secondarySelectedBackgroundColor: Color,
            secondarySelectedTextColor:       Color,
            bottomBorderColor:                Color,
            countBackgroundColor:             Color,
            countTextColor:                   Color
        ) : unit =
        customize [
            Theme.Rules(
                primaryBackgroundColor,
                primaryTextColor,
                primarySelectedBackgroundColor,
                primarySelectedTextColor,
                secondaryBackgroundColor,
                secondaryTextColor,
                secondarySelectedBackgroundColor,
                secondarySelectedTextColor,
                bottomBorderColor,
                countBackgroundColor,
                countTextColor
            )
        ]

    static member One
        (
            primaryBackgroundColor:           Color,
            primaryTextColor:                 Color,
            primarySelectedBackgroundColor:   Color,
            primarySelectedTextColor:         Color,
            secondaryBackgroundColor:         Color,
            secondaryTextColor:               Color,
            secondarySelectedBackgroundColor: Color,
            secondarySelectedTextColor:       Color,
            bottomBorderColor:                Color,
            countBackgroundColor:             Color,
            countTextColor:                   Color
        ) : Styles =
        Theme.Rules(
            primaryBackgroundColor,
            primaryTextColor,
            primarySelectedBackgroundColor,
            primarySelectedTextColor,
            secondaryBackgroundColor,
            secondaryTextColor,
            secondarySelectedBackgroundColor,
            secondarySelectedTextColor,
            bottomBorderColor,
            countBackgroundColor,
            countTextColor
        ) |> makeSheet

    static member Rules
        (
            primaryBackgroundColor:           Color,
            primaryTextColor:                 Color,
            primarySelectedBackgroundColor:   Color,
            primarySelectedTextColor:         Color,
            secondaryBackgroundColor:         Color,
            secondaryTextColor:               Color,
            secondarySelectedBackgroundColor: Color,
            secondarySelectedTextColor:       Color,
            bottomBorderColor:                Color,
            countBackgroundColor:             Color,
            countTextColor:                   Color
        ) : List<ISheetBuildingBlock> =
        [
            "item" => [
                borderBottom 1 bottomBorderColor
            ]
            && [
                "Primary" => [
                    backgroundColor primaryBackgroundColor
                ] && [
                    "selected" => [
                        backgroundColor primarySelectedBackgroundColor
                    ]
                ]

                "Secondary" => [
                    backgroundColor secondaryBackgroundColor
                ] && [
                    "selected" => [
                        backgroundColor secondarySelectedBackgroundColor
                    ]
                ]
            ]

            "count" => [
                backgroundColor countBackgroundColor
            ]

            "count-text" => [
                color countTextColor
            ]

            "text" => []
            && [
                "Primary" => [
                    color primaryTextColor
                ] && [
                    "selected" => [
                        color primarySelectedTextColor
                    ]
                ]

                "Secondary" => [
                    color secondaryTextColor
                ] && [
                    "selected" => [
                        color secondarySelectedTextColor
                    ]
                ]
            ]
        ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
    Theme.Rules (
        primaryBackgroundColor           = Color.White,
        primaryTextColor                 = Color.Black,
        primarySelectedBackgroundColor   = Color.DevPink,
        primarySelectedTextColor         = Color.Black,
        secondaryBackgroundColor         = Color.White,
        secondaryTextColor               = Color.Black,
        secondarySelectedBackgroundColor = Color.DevPink,
        secondarySelectedTextColor       = Color.Black,
        bottomBorderColor                = Color.Black,
        countBackgroundColor             = Color.DevRed,
        countTextColor                   = Color.White
    )
]))
