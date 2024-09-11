[<AutoOpen>]
module ReactXP.Components.Picker

open ReactXP.Helpers

open Fable.Core.JsInterop
open Fable.Core

[<StringEnum>]
type Mode =
| Head
| Middle
| Tail

type PickerPropsItem = {
    label: string
    value: string
}
type ReactXP.Components.Constructors.RX with
    static member Picker(
        items:          array<PickerPropsItem>,
        selectedValue:  string,
        onValueChange:  (* value *) string -> (* index *) int -> unit,
        ?mode:          Mode,
        ?styles:        array<ReactXP.Styles.FSharpDialect.ViewStyles>,
        ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>
    ) =
        let __props = createEmpty

        __props?items         <- items
        __props?selectedValue <- selectedValue
        __props?onValueChange <- onValueChange
        __props?mode          <- mode
        __props?style         <- styles

        match xLegacyStyles with
        | Option.None | Option.Some [] -> ()
        | Option.Some styles -> __props?__style <- styles

        Fable.React.ReactBindings.React.createElement(
            ReactXPRaw?Picker,
            __props,
            [||]
        )
