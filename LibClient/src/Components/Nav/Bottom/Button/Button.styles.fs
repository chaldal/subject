module LibClient.Components.Nav.Bottom.ButtonStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "button" => []
])

type (* class to enable named parameters *) Theme() =
    static member Customize = makeCustomize("LibClient.Components.Nav.Bottom.Button", baseStyles)

    static member BadgeColor (badgeColor: Color) : Styles =
        let blocks: List<ISheetBuildingBlock> =
            [
                "button" ==> LibClient.Components.ButtonStyles.Theme.BadgeColor(badgeColor)
            ]
        blocks |> makeSheet
        
let styles = lazy (compile (List.concat [
    baseStyles.Value
]))        