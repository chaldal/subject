module LibClient.Components.Input.ParsedTextRender

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

open LibClient.Components.Input.ParsedText



let render(children: array<ReactElement>, props: LibClient.Components.Input.ParsedText.Props<'T>, estate: LibClient.Components.Input.ParsedText.Estate<'T>, pstate: LibClient.Components.Input.ParsedText.Pstate, actions: LibClient.Components.Input.ParsedText.Actions<'T>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Input.Text"
    let __currClass = (System.String.Format("{0}{1}{2}", "theme ", (TopLevelBlockClass), ""))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    LibClient.Components.Constructors.LC.Input.Text(
        validity = (if props.ShouldShowValidationErrors then match props.Value.Result with Ok _ -> props.Validity | Error message -> InputValidity.Invalid message else InputValidity.Valid),
        ?tabIndex = (props.TabIndex),
        ?suffix = (props.Suffix),
        ?prefix = (props.Prefix),
        ?placeholder = (props.Placeholder),
        requestFocusOnMount = (props.RequestFocusOnMount),
        multiline = (false),
        ?onEnterKeyPress = (props.OnEnterKeyPress),
        ?onKeyPress = (props.OnKeyPress),
        onChange = (Value.OfRaw props.Parse >> props.OnChange),
        value = (getRawInputValueForFieldInternalProcessing props.Value),
        ?label = (props.Label),
        editable = (props.Editable),
        ?styles = (props.styles),
        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
    )
