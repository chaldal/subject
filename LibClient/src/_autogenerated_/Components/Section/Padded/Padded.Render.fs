module LibClient.Components.Section.PaddedRender

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

open LibClient.Components.Section.Padded



let render(children: array<ReactElement>, props: LibClient.Components.Section.Padded.Props, estate: LibClient.Components.Section.Padded.Estate, pstate: LibClient.Components.Section.Padded.Pstate, actions: LibClient.Components.Section.Padded.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.With.ScreenSize"
    LibClient.Components.Constructors.LC.With.ScreenSize(
        ``with`` =
            (fun (screenSize) ->
                    (castAsElementAckingKeysWarning [|
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "view ", (TopLevelBlockClass), " ", (screenSize.Class), ""))
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            children = (children),
                            ?styles =
                                (
                                    let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                                    match props.styles with
                                    | Some styles ->
                                        Array.append __currProcessedStyles styles |> Some
                                    | None -> Some __currProcessedStyles
                                )
                        )
                    |])
            )
    )
