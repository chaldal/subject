module LibClient.Components.Input.LocalTimeRender

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

open LibClient.Components.Input.LocalTime



let render(children: array<ReactElement>, props: LibClient.Components.Input.LocalTime.Props, estate: LibClient.Components.Input.LocalTime.Estate, pstate: LibClient.Components.Input.LocalTime.Pstate, actions: LibClient.Components.Input.LocalTime.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    (
        let externalValidityForFields = match props.Validity with Valid -> Valid | _ -> Missing
        let (rawHours, rawMinutes, rawPeriodOffset) = (
            props.Value.Raw;
             
        )
        let __parentFQN = Some "ReactXP.Components.View"
        let __currClass = "view"
        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
        ReactXP.Components.Constructors.RX.View(
            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
            children =
                [|
                    (
                        (props.Label)
                        |> Option.map
                            (fun label ->
                                let __parentFQN = Some "ReactXP.Components.View"
                                ReactXP.Components.Constructors.RX.View(
                                    onPress = (actions.Focus),
                                    children =
                                        [|
                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                            let __currClass = "label" + System.String.Format(" {0} {1}", (if ((props.Value.InternalValidity.Or props.Validity).IsInvalid) then "invalid" else ""), (if (estate.IsFocused) then "focused" else ""))
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.LegacyText(
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", label))
                                                    |]
                                            )
                                        |]
                                )
                            )
                        |> Option.getOrElse noElement
                    )
                    let __parentFQN = Some "ReactXP.Components.View"
                    let __currClass = "fields"
                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                    ReactXP.Components.Constructors.RX.View(
                        ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                        children =
                            [|
                                let __parentFQN = Some "LibClient.Components.Input.Text"
                                let __currClass = "field"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                LibClient.Components.Constructors.LC.Input.Text(
                                    ref = (actions.RefHoursInput),
                                    ?onEnterKeyPress = (props.OnEnterKeyPress),
                                    onBlur = (actions.OnBlur),
                                    onFocus = (actions.OnFocus),
                                    onChange = (props.Value.SetHours >> props.OnChange),
                                    requestFocusOnMount = (props.RequestFocusOnMount),
                                    placeholder = ("00"),
                                    validity = ((props.Value.InternalFieldValidity Hours).Or externalValidityForFields),
                                    maxLength = (2),
                                    value = (rawHours),
                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                )
                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                let __currClass = "colon"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                LibClient.Components.Constructors.LC.LegacyText(
                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                    children =
                                        [|
                                            makeTextNode2 __parentFQN ":"
                                        |]
                                )
                                let __parentFQN = Some "LibClient.Components.Input.Text"
                                let __currClass = "field"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                LibClient.Components.Constructors.LC.Input.Text(
                                    ?onEnterKeyPress = (props.OnEnterKeyPress),
                                    onBlur = (actions.OnBlur),
                                    onFocus = (actions.OnFocus),
                                    onChange = (props.Value.SetMinutes >> props.OnChange),
                                    placeholder = ("00"),
                                    validity = ((props.Value.InternalFieldValidity Minutes).Or externalValidityForFields),
                                    maxLength = (2),
                                    value = (rawMinutes),
                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                )
                                let __parentFQN = Some "LibClient.Components.Legacy.Input.Picker"
                                let __currClass = "picker"
                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                LibClient.Components.Constructors.LC.Legacy.Input.Picker(
                                    validity = (externalValidityForFields),
                                    onChange = (LibClient.Components.Legacy.Input.Picker.CannotUnselect (snd >> props.Value.SetPeriod >> props.OnChange)),
                                    value = (LibClient.Components.Legacy.Input.Picker.ByItem rawPeriodOffset |> Some),
                                    items = (periodPickerItems),
                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                )
                            |]
                    )
                    (
                        ((props.Value.InternalValidity.Or props.Validity).InvalidReason)
                        |> Option.map
                            (fun reason ->
                                let __parentFQN = Some "ReactXP.Components.View"
                                ReactXP.Components.Constructors.RX.View(
                                    children =
                                        [|
                                            let __parentFQN = Some "LibClient.Components.LegacyText"
                                            let __currClass = "invalid-reason"
                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                            LibClient.Components.Constructors.LC.LegacyText(
                                                ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                children =
                                                    [|
                                                        makeTextNode2 __parentFQN (System.String.Format("{0}", reason))
                                                    |]
                                            )
                                        |]
                                )
                            )
                        |> Option.getOrElse noElement
                    )
                |]
        )
    )
