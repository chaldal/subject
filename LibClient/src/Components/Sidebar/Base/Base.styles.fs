module LibClient.Components.Sidebar.BaseStyles

open ReactXP.LegacyStyles

let styles = lazy (compile  [
    "view" => [
        width            300
        borderColor      (Color.Grey "cc")
        borderLeftWidth  1
        borderRightWidth 1
        backgroundColor  Color.White
    ]

    "view" ==> LibClient.Components.VerticallyScrollableStyles.Theme.OneFixedTop [
        borderBottom 1 (Color.Grey "cc")
    ]

    "view" ==> LibClient.Components.VerticallyScrollableStyles.Theme.OneFixedBottom [
        borderTop 1 (Color.Grey "cc")
    ]
])

type (* class to enable named parameters *) Theme() =
    static member SetWrap (itemHeight: int) : List<ISheetBuildingBlock> =
        [
            "view" => [
                flexGrow itemHeight
            ]
        ]