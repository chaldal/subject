module LibClient.Components.Dialog.Shell.WhiteRounded.BaseStyles

open ReactXP.LegacyStyles

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Dialog.Shell.WhiteRounded.Base", lazy List.empty<ISheetBuildingBlock>)

    static member Width (value: int) : Styles = Theme.WidthRules value |> makeSheet

    static member WidthRules (value: int) : List<ISheetBuildingBlock> = [
        "dialog-raw" ==> LibClient.Components.Dialog.Shell.WhiteRounded.RawStyles.Theme.Width value
    ]

let styles = lazy RuntimeStyles.None