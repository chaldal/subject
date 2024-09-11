module LibClient.Components.Nav.Top.HeadingRender

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

open LibClient.Components.Nav.Top.Heading



let render(children: array<ReactElement>, props: LibClient.Components.Nav.Top.Heading.Props, estate: LibClient.Components.Nav.Top.Heading.Estate, pstate: LibClient.Components.Nav.Top.Heading.Pstate, actions: LibClient.Components.Nav.Top.Heading.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                        let __parentFQN = Some "LibClient.Components.Heading"
                        let __currClass = (System.String.Format("{0}{1}{2}{3}{4}", "view ", (TopLevelBlockClass), " ", (screenSize.Class), ""))
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        LibClient.Components.Constructors.LC.Heading(
                            ?styles = (props.styles),
                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                            children =
                                [|
                                    makeTextNode2 __parentFQN (System.String.Format("{0}", props.Text))
                                |]
                        )
                    |])
            )
    )
