module LibClient.Components.Legacy.SidebarTheme

open ReactXP.Styles

type (* class to enable named parameters *) Theme() =
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

        LibClient.Components.Legacy.Sidebar.ItemStyles.Theme.All (
            primaryBackgroundColor           = primaryBackgroundColor,
            primaryTextColor                 = primaryTextColor,
            primarySelectedBackgroundColor   = primarySelectedBackgroundColor,
            primarySelectedTextColor         = primarySelectedTextColor,
            secondaryBackgroundColor         = secondaryBackgroundColor,
            secondaryTextColor               = secondaryTextColor,
            secondarySelectedBackgroundColor = secondarySelectedBackgroundColor,
            secondarySelectedTextColor       = secondarySelectedTextColor,
            bottomBorderColor                = bottomBorderColor,
            countBackgroundColor             = countBackgroundColor,
            countTextColor                   = countTextColor
        )

        LibClient.Components.Legacy.Sidebar.FillerStyles.Theme.All (
            bottomBorderColor = bottomBorderColor
        )
