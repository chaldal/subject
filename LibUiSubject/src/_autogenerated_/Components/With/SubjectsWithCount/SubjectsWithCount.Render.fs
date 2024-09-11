module LibUiSubject.Components.With.SubjectsWithCountRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibUiSubject.Components
open LibUiSubject.Components

open LibClient
open LibClient.RenderHelpers

open LibUiSubject.Components.With.SubjectsWithCount
open LibUiSubject


let render(children: array<ReactElement>, props: LibUiSubject.Components.With.SubjectsWithCount.Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, estate: LibUiSubject.Components.With.SubjectsWithCount.Estate<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, pstate: LibUiSubject.Components.With.SubjectsWithCount.Pstate, actions: LibUiSubject.Components.With.SubjectsWithCount.Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Subscribe"
    LibClient.Components.Constructors.LC.Subscribe(
        ``with`` = (match props.With with WithSubjectsWithCount.Raw f -> LibClient.Components.Subscribe.Raw f | WithSubjectsWithCount.WhenAvailable f -> LibClient.Components.Subscribe.WhenAvailable f),
        subscription = (props.Subscription),
        subscriptionKey = (props.By.ToString())
    )
