module LibClient.Components.VerticallyScrollableStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "view" => [
        flex 1
    ]

    "top" => [
        flex 0
    ]

    "middle" => [
        flex 1
    ]

    "bottom" => [
        flex 0
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Tabs", baseStyles)

    static member OneFixedTop (rules: seq<RuleFunctionReturnedStyleRules>) : Styles =
        let rules: list<ISheetBuildingBlock> = [ "top" => rules ]
        rules |> makeSheet

    static member OneFixedBottom (rules: seq<RuleFunctionReturnedStyleRules>) : Styles =
        let rules: list<ISheetBuildingBlock> = [ "bottom" => rules ]
        rules |> makeSheet

    static member OneFixedMiddle (rules: seq<RuleFunctionReturnedStyleRules>) : Styles =
        let rules: list<ISheetBuildingBlock> = [ "middle" => rules ]
        rules |> makeSheet

    static member AllFixedTop (rules: seq<RuleFunctionReturnedStyleRules>) : unit =
        customize [[ "top" => rules ]]

    static member AllFixedBottom (rules: seq<RuleFunctionReturnedStyleRules>) : unit =
        customize [[ "bottom" => rules ]]

    static member AllScrollableMiddle (rules: seq<RuleFunctionReturnedStyleRules>) : unit =
        customize [[ "middle" => rules ]]

let styles = lazy (compile baseStyles.Value)
