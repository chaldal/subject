module LibClient.Components.Dialog.Shell.WhiteRounded.BaseRender

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

open LibClient.Components.Dialog.Shell.WhiteRounded.Base



let render(children: array<ReactElement>, props: LibClient.Components.Dialog.Shell.WhiteRounded.Base.Props, estate: LibClient.Components.Dialog.Shell.WhiteRounded.Base.Estate, pstate: LibClient.Components.Dialog.Shell.WhiteRounded.Base.Pstate, actions: LibClient.Components.Dialog.Shell.WhiteRounded.Base.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Dialog.Shell.WhiteRounded.Raw"
    let __currClass = "dialog-raw"
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    LibClient.Components.Constructors.LC.Dialog.Shell.WhiteRounded.Raw(
        inProgress = (props.InProgress),
        canClose = (props.CanClose),
        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
        children =
            [|
                let __parentFQN = Some "ReactXP.Components.ScrollView"
                let __currClass = "scroll-view"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                ReactXP.Components.Constructors.RX.ScrollView(
                    children = (children),
                    vertical = (true),
                    ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.ScrollView" __currStyles |> Some) else None)
                )
            |]
    )
