[<AutoOpen>]
module LibClient.ButtonHighLevelStateTypeExtensions

open LibRouter.RoutesSpec
open LibRouter.Components.With.Navigation

type LibClient.Input.ButtonHighLevelStateFactory with
    static member MakeGo<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality and 'Route :> NavigationRoute and 'ResultlessDialog :> NavigationResultlessDialog and 'ResultfulDialog :> NavigationResultfulDialog> (route: 'Route, nav: Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog>) : ButtonHighLevelState =
        nav.Go route
        |> ButtonLowLevelState.Actionable
        |> ButtonHighLevelState.LowLevel

    static member MakeGo<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality and 'Route :> NavigationRoute and 'ResultlessDialog :> NavigationResultlessDialog and 'ResultfulDialog :> NavigationResultfulDialog> (dialog: 'ResultlessDialog, nav: Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog>) : ButtonHighLevelState =
        nav.Go dialog
        |> ButtonLowLevelState.Actionable
        |> ButtonHighLevelState.LowLevel

    static member MakeGo<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality and 'Route :> NavigationRoute and 'ResultlessDialog :> NavigationResultlessDialog and 'ResultfulDialog :> NavigationResultfulDialog> (dialog: SystemDialog, nav: Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog>) : ButtonHighLevelState =
        nav.Go dialog
        |> ButtonLowLevelState.Actionable
        |> ButtonHighLevelState.LowLevel

    static member MakeGo<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality and 'Route :> NavigationRoute and 'ResultlessDialog :> NavigationResultlessDialog and 'ResultfulDialog :> NavigationResultfulDialog> (dialog: 'ResultfulDialog, nav: Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog>) : ButtonHighLevelState =
        nav.Go dialog
        |> ButtonLowLevelState.Actionable
        |> ButtonHighLevelState.LowLevel

    static member MakeGoExternal<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality and 'Route :> NavigationRoute and 'ResultlessDialog :> NavigationResultlessDialog and 'ResultfulDialog :> NavigationResultfulDialog> (url: string, nav: Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog>) : ButtonHighLevelState =
        (fun _e -> nav.GoExternal url)
        |> ButtonLowLevelState.Actionable
        |> ButtonHighLevelState.LowLevel

type ButtonHighLevelState with
    static member DoWithConfirm<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality and 'Route :> NavigationRoute and 'ResultlessDialog :> NavigationResultlessDialog and 'ResultfulDialog :> NavigationResultfulDialog> (
        action: UDAction,
        nav:    Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog>,
        ?message: string,
        ?cancelButtonLabel: string,
        ?confirmButtonLabel: string
    ) : ButtonHighLevelState =
        Input.ButtonHighLevelStateFactory.MakeGo (
            ConfirmAsync (
                None,
                message            |> Option.getOrElse "Are you sure?",
                cancelButtonLabel  |> Option.getOrElse "Cancel",
                confirmButtonLabel |> Option.getOrElse "Ok",
                action
            ),
            nav
        )

    static member Navigate<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality and 'Route :> NavigationRoute and 'ResultlessDialog :> NavigationResultlessDialog and 'ResultfulDialog :> NavigationResultfulDialog> (
        navigationFunction: ReactEvent.Action -> unit
    ) : ButtonHighLevelState =
        navigationFunction
        |> ButtonLowLevelState.Actionable
        |> ButtonHighLevelState.LowLevel
