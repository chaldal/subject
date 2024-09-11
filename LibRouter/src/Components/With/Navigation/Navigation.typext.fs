module LibRouter.Components.With.Navigation

open LibClient
open LibClient.Dialogs
open LibRouter.RoutesSpec
open LibClient.ServiceInstances
open LibRouter.Components
open Fable.Core.JsInterop

type Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality>(spec: LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>, navigationState: NavigationState<'Route, 'ResultlessDialog, 'ResultfulDialog>, history: Router.History) =
    let goUrlInNewTab (url: string) : unit =
        if ReactXP.Runtime.isWeb() then
            Browser.Dom.window?``open``(url, "_blank")
        else
            ReactXP.Linking.openUrl url

    let makeExternalUrl (appUrlBase: string) (location: Location) : string =
        appUrlBase + location.ToString

    member this.GoInSameTab (dialog: 'ResultlessDialog) : unit =
        match this.CurrentFrame with
        | Some currFrame ->
            let token = navigationState.DialogsState.AddResultless ()
            this.GoInSameTab { currFrame with Dialogs = (NavigationDialog.Resultless (token, dialog)) :: currFrame.Dialogs }
        | None -> failwith "Cannot open a dialog when we don't have a background route"

    member this.GoInSameTab (dialog: 'ResultfulDialog) : unit =
        match this.CurrentFrame with
        | Some currFrame ->
            let token = navigationState.DialogsState.AddResultful dialog
            this.GoInSameTab { currFrame with Dialogs = (NavigationDialog.Resultful token) :: currFrame.Dialogs }
        | None -> failwith "Cannot open a dialog when we don't have a background route"

    member this.Redirect (route: 'Route) : unit =
        route
        |> NavigationFrame.ofRoute
        |> this.Redirect

    member _.Redirect (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : unit =
        history.replace (spec.ToLocation navigationFrame).ToString

    member this.GoInSameTab (route: 'Route) : unit =
        route
        |> NavigationFrame.ofRoute
        |> this.GoInSameTab

    member _.GoInSameTab (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : unit =
        history.push (spec.ToLocation navigationFrame).ToString

    member this.GoInSameTab (location: Location) : unit =
        let maybeNavigationFrame = location |> spec.FromLocation

        match maybeNavigationFrame with
        | Some navigationFrame -> this.GoInSameTab navigationFrame
        | None                 -> this.GoExternalInSameTab (makeExternalUrl spec.AppBaseUrl location)

    member this.GoInSameTab (url: string) : unit =
        url
        |> Url.tryParse
        |> Option.sideEffect (fun parsedUrl ->
            let maybeNavigationFrame =
                { Path = parsedUrl.Path; Query = parsedUrl.Fragment }
                |> spec.FromLocation

            match maybeNavigationFrame with
            | Some navigationFrame -> this.GoInSameTab navigationFrame
            | None                 -> this.GoExternalInSameTab url
        )

    member _.GoInNewTab (route: 'Route) : unit =
        route |> NavigationFrame.ofRoute |> spec.ToLocation |> (makeExternalUrl spec.AppBaseUrl) |> goUrlInNewTab

    member _.GoExternal (url: string) : unit =
        goUrlInNewTab url

    member _.GoExternalInSameTab (url: string) : unit =
        if ReactXP.Runtime.isWeb() then
            Browser.Dom.window?``open``(url, "_self")
        else
            ReactXP.Linking.openUrl url

    member _.Reload () : unit =
        Browser.Dom.window?location?reload()

    member this.GoBack () : unit =
        navigationState.Navigate NavigationDirection.Back this.CurrentFrame

        // we reset because possibly not all routes set the page title,
        // which would cause the old title mismatching the new page to linger
        services().PageTitle.ResetRouteName()
        history.goBack()

    member this.Go (route: 'Route) : ReactEvent.Action -> unit =
        route
        |> NavigationFrame.ofRoute
        |> this.Go

    member this.Go (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : ReactEvent.Action -> unit =
        fun (e: ReactEvent.Action) ->
            let newLocation = spec.ToLocation navigationFrame
            // Fable can't match on types of interfaces at runtime, so we can't
            // downcast to Browser.Types.PointerEvent to inspect these fields.
            let isCtrlOrMetaPressed: bool = e.MaybeEvent |> Option.map (fun e -> e?metaKey || e?ctrlKey || false) |> Option.getOrElse false

            if isCtrlOrMetaPressed then
                goUrlInNewTab (makeExternalUrl spec.AppBaseUrl newLocation)
            else
                navigationState.Navigate NavigationDirection.Forward this.CurrentFrame

                // we reset because possibly not all routes set the page title,
                // which would cause the old title mismatching the new page to linger
                services().PageTitle.ResetRouteName()
                history.push newLocation.ToString

    member this.Replace (route: 'Route) : ReactEvent.Action -> unit =
        route
        |> NavigationFrame.ofRoute
        |> this.Replace

    member this.Replace (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : ReactEvent.Action -> unit =
        fun (e: ReactEvent.Action) ->
            let newLocation = spec.ToLocation navigationFrame
            // Fable can't match on types of interfaces at runtime, so we can't
            // downcast to Browser.Types.PointerEvent to inspect these fields.
            let isCtrlOrMetaPressed: bool = e.MaybeEvent |> Option.map (fun e -> e?metaKey || e?ctrlKey || false) |> Option.getOrElse false

            if isCtrlOrMetaPressed then
                goUrlInNewTab (makeExternalUrl spec.AppBaseUrl newLocation)
            else
                navigationState.Navigate NavigationDirection.Replace this.CurrentFrame
                // we reset because possibly not all routes set the page title,
                // which would cause the old title mismatching the new page to linger
                services().PageTitle.ResetRouteName()
                history.replace newLocation.ToString

    member this.GoExternalMaybeInNewTab (url: string) (e: ReactEvent.Action) : unit =
        // Fable can't match on types of interfaces at runtime, so we can't
        // downcast to Browser.Types.PointerEvent to inspect these fields.
        let isCtrlOrMetaPressed: bool = e.MaybeEvent |> Option.map (fun e -> e?metaKey || e?ctrlKey || false) |> Option.getOrElse false

        if isCtrlOrMetaPressed then
            goUrlInNewTab url
        else
            this.GoExternalInSameTab url

    member private this.GoToDialog (navigationDialog: NavigationDialog<'ResultlessDialog>) (e: ReactEvent.Action) : unit =
        match this.CurrentFrame with
        | Some currFrame ->
            this.Go { currFrame with Dialogs = navigationDialog :: currFrame.Dialogs } e
        | None -> failwith "Cannot open a dialog when we don't have a background route"

    member this.Go (closeToAdHocDialog: (DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement) : ReactEvent.Action -> unit =
        fun e ->
            let token = navigationState.DialogsState.AddAdHoc closeToAdHocDialog
            this.GoToDialog (NavigationDialog.AdHoc token) e

    member this.Go (dialog: 'ResultfulDialog) : ReactEvent.Action -> unit =
        // NOTE very important to have the wrapper
        fun e ->
            let token = navigationState.DialogsState.AddResultful dialog
            this.GoToDialog (NavigationDialog.Resultful token) e

    member this.Go (dialog: 'ResultlessDialog) : ReactEvent.Action -> unit =
        // NOTE very important to have the wrapper
        fun e ->
            let token = navigationState.DialogsState.AddResultless ()
            this.GoToDialog (NavigationDialog.Resultless (token, dialog)) e

    member private _.CurrentFrame : Option<NavigationFrame<'Route, 'ResultlessDialog>> =
        spec.FromLocation { Path = history.location.pathname; Query = history.location.search }

    member this.CurrentRoute : Option<'Route> =
        this.CurrentFrame |> Option.map NavigationFrame.route

    member this.IsCurrentRoute (route: 'Route) : bool =
        // NOTE compare 'Routes, not urls, since different urls
        // can map to the same route, as is often the case with / and /home
        this.CurrentRoute = Some route

    member this.Close (token: OpenDialogToken) (method: DialogCloseMethod) (e: ReactEvent.Action) : unit =
        match this.CurrentFrame with
        | Some currFrame ->
            match method with
            | DialogCloseMethod.HistoryForward ->
                let updatedDialogs = currFrame.Dialogs |> List.filterNot (fun dialog -> dialog.Token = token)
                this.Go { currFrame with Dialogs = updatedDialogs } e
            | DialogCloseMethod.HistoryBack ->
                this.GoBack ()

        | None -> Noop


type Props<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality> = (* GenerateMakeFunction *) {
    Spec:            LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>
    NavigationState: LibRouter.RoutesSpec.NavigationState<'Route, 'ResultlessDialog, 'ResultfulDialog>
    With:            Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog> -> ReactElement

    key: string option // defaultWithAutoWrap JsUndefined
}


let renderFn (props: Props<'Route, 'ResultlessDialog, 'ResultfulDialog>) : ReactElement =
    props.With (Navigation (props.Spec, props.NavigationState, Router.useHistory()))

let Make<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality> = 
    makeFnConstructor "LibRouter.Components.With.Navigation" renderFn

// Unfortunately necessary boilerplate
type Actions<'Route, 'ResultlessDialog, 'ResultfulDialog> = NoActions
type Estate<'Route, 'ResultlessDialog, 'ResultfulDialog> = NoEstate3<'Route, 'ResultlessDialog, 'ResultfulDialog>
type Pstate = NoPstate
