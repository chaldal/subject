module LibUiAdmin.Components.GridStyles

open LibClient.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let navPageNumber =
        makeViewStyles {
            marginHorizontal 10
        }

    let navCurrentPage =
        makeViewStyles {
            borderWidth 1
            borderRadius 2
            backgroundColor Color.White
            paddingHorizontal 5
            paddingVertical 2
        }

    let navButtonTheme (theme: LC.IconButton.Theme): LC.IconButton.Theme =
        { theme with
            Actionable =
                { theme.Actionable with
                    IconSize = 14
                }
            Disabled =
                { theme.Disabled with
                    IconSize = 14
                }
            InProgress =
                { theme.InProgress with
                    IconSize = 14
                }
        }

    let navButton =
        makeViewStyles {
            marginHorizontal 5
        }

open ReactXP.LegacyStyles
open LibClient.ColorScheme

let private columnBaseWidth = 20

// NOTE this is labelled as a hack, because the correct solution to styles sharing
// would involve a single instance of the styles defined, but this takes some framework
// level programming which is out of scope at the moment, so for now, we are going to
// duplicate these styles at every use site by pulling them in and dropping them inside
// the `compile [ ... ]` list.
let private hackColumnWidthStyles: List<Selector * Styles> =
    seq { 1 .. 20 }
    |> Seq.map (fun i -> (sprintf "col-w-%i" i, rules [ width (columnBaseWidth * i) ]))
    |> Seq.toList

let private hackColumnAlignmentStyles: List<Selector * Styles> =
    [
        ("col-ha-center", rules [
            JustifyContent.Center
            TextAlign.Center
        ])
        ("col-ha-right", rules [
            JustifyContent.FlexEnd
            TextAlign.Right
        ])
        ("col-ha-left", rules [
            JustifyContent.FlexStart
            TextAlign.Left
        ])
    ]

let private hackRowStyles =
    [
        ("row-alt", rules [
            backgroundColor (Color.Grey "f0")
        ])
    ]

let hackGridStyles =
    hackColumnWidthStyles @ hackColumnAlignmentStyles @ hackRowStyles
    |> SheetBuildingBlockMany

let private localStyles = lazy (compile [
    "view" => [
        marginTop    20
        marginBottom 40
    ]

    "full-height" => [
        flex 1
    ]

    "scroll-view-horizontal" => [
        paddingVertical 2
        minHeight       160
    ]

    "scroll-view-vertical" => [
        paddingHorizontal 2
        minWidth          160
        flex              1
    ]

    "headers" => [
        FlexDirection.Row
        Overflow.Visible
    ]

    "rows" => [
        Overflow.Visible
        minHeight 100 // cheap way of making the spinner visible for `Fetching None` case
    ]

    "empty-message" => [
        margin 42
    ]

    "empty-message-text" => [
        TextAlign.Center
    ]

    "pagination" => [
        paddingHorizontal 10
        backgroundColor   (Color.Grey "f0")
        FlexDirection.Row
        AlignItems.Stretch
        JustifyContent.SpaceBetween
    ]

    "paginationHandheld" => [
        paddingHorizontal 10
        AlignItems.Center
    ]

    "navigation" => [
        FlexDirection.Row
        AlignItems.Center
        flex 0
    ]

    "page-size" => [
        flex 0
        AlignItems.Center
        FlexDirection.Row
        JustifyContent.FlexEnd
    ]

    "page-size-info" => [
        marginHorizontal 10
        fontSize 12
    ]

    "page-size-text" => [
        marginHorizontal 10
    ]

    "picker" => [
        padding         0
        margin          10
        backgroundColor Color.White
    ]

    "page-info-container" => [
        FlexDirection.Row
        AlignItems.Center
    ]

    "goto-page-btn" => [
        fontSize 16
        height   40
    ]

    "goto-page-input" => [
        marginTop       0
        width           80
        backgroundColor Color.White
    ]

    "curr-page-info" => [
        marginHorizontal 10
    ]

    "nav-page-internal-dot" => [
        marginHorizontal 10
        color            MaterialDesignColors.Indigo.Main
    ]

    "result-count" => [
        marginLeft 10
    ]

    hackGridStyles
])

addCss("""
.row {
    position: relative
}

.row-alt {
    background-color: #f0f0f0;
}
""")

let styles = RuntimeStyles.FixmeCrappyStyleSharing localStyles LibUiAdmin.Styles.styles
