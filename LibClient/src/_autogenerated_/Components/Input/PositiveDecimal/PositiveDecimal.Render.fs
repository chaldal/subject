module LibClient.Components.Input.PositiveDecimalRender

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

open LibClient.Components.Input.PositiveDecimal



let render(children: array<ReactElement>, props: LibClient.Components.Input.PositiveDecimal.Props, estate: LibClient.Components.Input.PositiveDecimal.Estate, pstate: LibClient.Components.Input.PositiveDecimal.Pstate, actions: LibClient.Components.Input.PositiveDecimal.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Input.ParsedText"
    let __currClass = (System.String.Format("{0}", TopLevelBlockClass))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    LibClient.Components.Constructors.LC.Input.ParsedText(
        ?styles = (props.styles),
        keyboardType = (LibClient.Components.Input.ParsedText.KeyboardType.Numeric),
        ?onEnterKeyPress = (props.OnEnterKeyPress),
        ?onKeyPress = (props.OnKeyPress),
        onChange = (props.OnChange),
        ?tabIndex = (props.TabIndex),
        requestFocusOnMount = (props.RequestFocusOnMount),
        validity = (props.Validity),
        ?suffix = (props.Suffix),
        ?prefix = (props.Prefix),
        ?placeholder = (props.Placeholder),
        value = (props.Value),
        ?label = (props.Label),
        parse = (parseProp),
        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
    )
