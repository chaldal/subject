module LibClient.Components.Dialog.Shell.FullScreenStyles

open LibClient.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
module Styles =
    let iconButtonTheme (theme: LC.Legacy.TopNav.IconButtonTypes.Theme): LC.Legacy.TopNav.IconButtonTypes.Theme =
        { theme with IconColor = (Color.Grey "99") }

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "wrapper" => [
        Position.Absolute
        FlexDirection.ColumnReverseZindexHack
        trbl            0 0 0 0
        backgroundColor Color.White
    ]

    "children" => [
        flex 1
    ]

    "scroll-view-children" => [
        flex 1
    ]

    "top-nav" ==> LibClient.Components.Legacy.TopNav.BaseStyles.Theme.One (theBackgroundColor = Color.White, theTextColor = Color.Grey "77")


])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Dialog.Shell.FullScreen", baseStyles)

    static member All (?theTopNavHeight: int) : unit =
        let topNavHeight = defaultArg theTopNavHeight 44

        customize [
            Theme.TopNavHeight topNavHeight
        ]

    static member TopNavHeight (theTopNavHeight: int) : List<ISheetBuildingBlock> = [
        "top-nav" ==> LibClient.Components.Legacy.TopNav.BaseStyles.Theme.Height theTopNavHeight
    ]

let styles = lazy (compile (List.concat [
    baseStyles.Value
]))
