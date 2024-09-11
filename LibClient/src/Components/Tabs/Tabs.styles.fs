module LibClient.Components.TabsStyles

open ReactXP.LegacyStyles

let private baseStyles = lazy (asBlocks [
    "scroll-view" => [
        Overflow.Visible
        flex 0
    ]

    "view" => [
        FlexDirection.Row
        AlignContent.Stretch
        Overflow.Visible
    ]
])

type (* class to enable named parameters *) Theme() =
    static let customize = makeCustomize("LibClient.Components.Tabs", baseStyles)

    static member Customize (rules: seq<RuleFunctionReturnedStyleRules>) : unit =
        customize [[
            "scroll-view" => rules
        ]]

let styles = lazy (compile baseStyles.Value)
