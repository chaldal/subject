module LibClient.Components.Input.DateRender

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

open LibClient.Components.Input.Date



let render(children: array<ReactElement>, props: LibClient.Components.Input.Date.Props, estate: LibClient.Components.Input.Date.Estate, pstate: LibClient.Components.Input.Date.Pstate, actions: LibClient.Components.Input.Date.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = (System.String.Format("{0}", TopLevelBlockClass))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles =
            (
                let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                match props.styles with
                | Some styles ->
                    Array.append __currProcessedStyles styles |> Some
                | None -> Some __currProcessedStyles
            ),
        children =
            [|
                let __parentFQN = Some "LibClient.Components.Input.ParsedText"
                let __currClass = "parsed-text"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                LibClient.Components.Constructors.LC.Input.ParsedText(
                    ?onEnterKeyPress = (props.OnEnterKeyPress),
                    ?onKeyPress = (props.OnKeyPress),
                    onChange = (props.OnChange),
                    requestFocusOnMount = (props.RequestFocusOnMount),
                    validity = (props.Validity),
                    placeholder = (props.Placeholder |> Option.defaultValue props.ValueFormat),
                    value = (props.Value),
                    ?label = (props.Label),
                    parse = (parseProp props.MinDate props.MaxDate props.CanSelectDate),
                    suffix =
                        (
                            InputSuffix.Element
                                (
                                        (castAsElementAckingKeysWarning [|
                                            let __parentFQN = Some "LibClient.Components.IconButton"
                                            let __currClass = "icon-button"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.IconButton(
                                                state = (LibClient.Components.IconButton.PropStateFactory.MakeLowLevel(LibClient.Components.IconButton.Actionable actions.Toggle)),
                                                icon = (Icon.Calendar),
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                            )
                                        |])
                                )
                        ),
                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                )
                let __parentFQN = Some "LibClient.Components.Popup"
                LibClient.Components.Constructors.LC.Popup(
                    connector = (estate.PopupConnector),
                    render =
                        (fun (_) ->
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "ReactXP.Components.View"
                                    let __currClass = "popup"
                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                    ReactXP.Components.Constructors.RX.View(
                                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                        children =
                                            [|
                                                (*  XXX it looks like the component is rendered once and then cut
                 off from the world, i.e. updates to props.Value do not propagate
                 to it. I think that's how ReactXP's popups infrastructure is set up,
                 though it seems strange and unlikely.  *)
                                                let __parentFQN = Some "LibClient.Components.DateSelector"
                                                let __currClass = "datepicker"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                LibClient.Components.Constructors.LC.DateSelector(
                                                    maybeSelected = (match props.Value.Result with | Ok result -> result | Error _ -> None),
                                                    ?canSelectDate = (props.CanSelectDate),
                                                    ?maxDate = (props.MaxDate),
                                                    ?minDate = (props.MinDate),
                                                    onChange = (fun date -> wrap props.ValueFormat date |> props.OnChange; estate.PopupConnector.Hide()),
                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                )
                                            |]
                                    )
                                |])
                        )
                )
            |]
    )
