module LibClient.Components.HeadingRender

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

open LibClient.Components.Heading



let render(children: array<ReactElement>, props: LibClient.Components.Heading.Props, estate: LibClient.Components.Heading.Estate, pstate: LibClient.Components.Heading.Pstate, actions: LibClient.Components.Heading.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
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
                        let __parentFQN = Some "LibClient.Components.LegacyText"
                        let __currClass = (System.String.Format("{0}{1}{2}{3}{4}{5}{6}", "text level-", (props.Level), " ", (screenSize.Class), " ", (TopLevelBlockClass), ""))
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        LibClient.Components.Constructors.LC.LegacyText(
                            children = (children),
                            ?styles = (props.styles),
                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                        )
                    |])
            )
    )
