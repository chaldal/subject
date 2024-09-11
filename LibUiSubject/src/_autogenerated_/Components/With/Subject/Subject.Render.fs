module LibUiSubject.Components.With.SubjectRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibUiSubject.Components
open LibUiSubject.Components

open LibClient
open LibClient.RenderHelpers

open LibUiSubject.Components.With.Subject
open LibUiSubject


let render(children: array<ReactElement>, props: LibUiSubject.Components.With.Subject.Props<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, estate: LibUiSubject.Components.With.Subject.Estate<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, pstate: LibUiSubject.Components.With.Subject.Pstate, actions: LibUiSubject.Components.With.Subject.Actions<'Subject, 'Projection, 'Id, 'Index, 'Constructor, 'Action, 'Event, 'OpError>, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.Subscribe"
    LibClient.Components.Constructors.LC.Subscribe(
        ``with`` = (match props.With with WithSubject.Raw f -> LibClient.Components.Subscribe.Raw f | WithSubject.WhenAvailable f -> LibClient.Components.Subscribe.WhenAvailable f),
        subscription = (fun subscriber -> props.Service.SubscribeOne props.Id props.UseCache subscriber),
        key = (props.Id.ToString())
    )
