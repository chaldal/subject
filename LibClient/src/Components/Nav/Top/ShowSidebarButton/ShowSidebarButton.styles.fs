module LibClient.Components.Nav.Top.ShowSidebarButtonStyles

open ReactXP.LegacyStyles

type Colors = LibClient.Components.Nav.Top.ItemStyles.Colors
type State = LibClient.Components.Nav.Top.Item.State

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Nav.Top.ShowSidebarButton", lazy [])

    static member One (state: State, baseColors: Colors, hoveredColors: Colors, depressedColors: Colors) : Styles =
        let blocks : List<ISheetBuildingBlock> = [
            "topnav-item" ==> (LibClient.Components.Nav.Top.ItemStyles.Theme.Part(state, baseColors, hoveredColors, depressedColors) |> makeSheet)
        ]
        blocks |> makeSheet

    static member ItemWidth(itemWidth: int) : List<ISheetBuildingBlock> =
        [
            "topnav-item" ==> (LibClient.Components.Nav.Top.ItemStyles.Theme.ItemWidth(itemWidth) |> makeSheet)
        ]

let styles = lazy (compile [
    "topnav-item" => [
        borderWidth 0
    ]
])
