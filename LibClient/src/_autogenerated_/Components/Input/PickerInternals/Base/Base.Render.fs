module LibClient.Components.Input.PickerInternals.BaseRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibClient.Components

open LibClient
open LibClient.RenderHelpers
open LibClient.Icons
open LibClient.Chars
open LibClient.ColorModule
open LibClient.LocalImages
open LibClient.Responsive

open LibClient.Components.Input.PickerInternals.Base



let render(children: array<ReactElement>, props: LibClient.Components.Input.PickerInternals.Base.Props<'Item>, estate: LibClient.Components.Input.PickerInternals.Base.Estate<'Item>, pstate: LibClient.Components.Input.PickerInternals.Base.Pstate, actions: LibClient.Components.Input.PickerInternals.Base.Actions<'Item>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Input.PickerInternals.Field"
    let __currClass = (System.String.Format("{0}", TopLevelBlockClass))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    LibClient.Components.Constructors.LC.Input.PickerInternals.Field(
        itemView = (props.ItemView),
        validity = (props.Validity),
        value = (props.Value),
        ?placeholder = (props.Placeholder),
        ?label = (props.Label),
        model = (actions.GetModel ()),
        ?styles = (props.styles),
        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
    )
