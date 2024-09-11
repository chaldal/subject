module LibRouter.Components.With.Navigation

open LibClient
open LibClient.Dialogs
open LibClient.SystemDialogs
open LibClient.EventBus
open LibRouter.RoutesSpec
open LibClient.ServiceInstances
open LibRouter.Components
open Fable.Core.JsInterop

[<RequireQualifiedAccess>]
type NavigationAction<'Route, 'ResultlessDialog, 'ResultfulDialog> =
| GoInSameTab_ResultlessDialog of 'ResultlessDialog
| GoInSameTab_ResultfulDialog  of 'ResultfulDialog
| GoInSameTab_SystemDialog     of SystemDialog
| GoInSameTab_Route            of 'Route
| GoInSameTab_Location         of Location
| GoInSameTab_Frame            of NavigationFrame<'Route, 'ResultlessDialog>
| GoInSameTab_Url              of Url: string
| Redirect                     of 'Route
| GoInNewTab                   of 'Route
| GoExternal                   of Url: string
| GoExternalInSameTab          of Url: string
| Go                           of 'Route * ReactEvent.Action
| Replace                      of 'Route * ReactEvent.Action
| ReplaceInSameTab             of 'Route
| GoExternalMaybeInNewTab      of Url: string * ReactEvent.Action
| Go_AdHocDialog               of CloseToAdHocDialog: ((DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement) * ReactEvent.Action
| Go_ResultfulDialog           of 'ResultfulDialog * ReactEvent.Action
| Go_ResultlessDialog          of 'ResultlessDialog * ReactEvent.Action
| Go_SystemDialog              of SystemDialog * ReactEvent.Action
| Go_Frame                     of NavigationFrame<'Route, 'ResultlessDialog> * ReactEvent.Action
| Close                        of OpenDialogToken * DialogCloseMethod * ReactEvent.Action
| Reload
| GoBack

type Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality>(queue: Queue<NavigationAction<'Route, 'ResultlessDialog, 'ResultfulDialog>>) =
    member private _.Broadcast (action: NavigationAction<'Route, 'ResultlessDialog, 'ResultfulDialog>) : unit =
        LibClient.ServiceInstances.services().EventBus.Broadcast queue action

    member this.GoInSameTab (dialog: 'ResultlessDialog) : unit =
        NavigationAction.GoInSameTab_ResultlessDialog dialog |> this.Broadcast

    member this.GoInSameTab (dialog: 'ResultfulDialog) : unit =
        NavigationAction.GoInSameTab_ResultfulDialog dialog |> this.Broadcast

    member this.GoInSameTab (dialog: SystemDialog) : unit =
        NavigationAction.GoInSameTab_SystemDialog dialog |> this.Broadcast

    member this.GoInSameTab (location: Location) : unit =
        NavigationAction.GoInSameTab_Location location |> this.Broadcast

    member this.GoInSameTab (frame: NavigationFrame<'Route, 'ResultlessDialog>) : unit =
        NavigationAction.GoInSameTab_Frame frame |> this.Broadcast

    member this.Redirect (route: 'Route) : unit =
        NavigationAction.Redirect route |> this.Broadcast

    member this.GoInSameTab (route: 'Route) : unit =
        NavigationAction.GoInSameTab_Route route |> this.Broadcast

    member this.GoInSameTab (url: string) : unit =
        NavigationAction.GoInSameTab_Url url |> this.Broadcast

    member this.GoInNewTab (route: 'Route) : unit =
        NavigationAction.GoInNewTab route |> this.Broadcast

    member this.GoExternal (url: string) : unit =
        NavigationAction.GoExternal url |> this.Broadcast

    member this.GoExternalInSameTab (url: string) : unit =
        NavigationAction.GoExternalInSameTab url |> this.Broadcast

    member this.Reload () : unit =
        NavigationAction.Reload |> this.Broadcast

    member this.GoBack () : unit =
        NavigationAction.GoBack |> this.Broadcast

    member this.Go (route: 'Route) =
        fun (e: ReactEvent.Action) ->
            NavigationAction.Go (route, e) |> this.Broadcast

    member this.Go (frame: NavigationFrame<'Route, 'ResultlessDialog>) =
        fun (e: ReactEvent.Action) ->
            NavigationAction.Go_Frame (frame, e) |> this.Broadcast

    member this.Replace (route: 'Route) : ReactEvent.Action -> unit =
        fun (e: ReactEvent.Action) ->
            NavigationAction.Replace (route, e) |> this.Broadcast

    member this.ReplaceInSameTab (route: 'Route) : unit =
        NavigationAction.ReplaceInSameTab route |> this.Broadcast

    member this.GoExternalMaybeInNewTab (url: string) (e: ReactEvent.Action) : unit =
        NavigationAction.GoExternalMaybeInNewTab (url, e) |> this.Broadcast

    member this.Go (closeToAdHocDialog: (DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement) =
        fun (e: ReactEvent.Action) ->
            NavigationAction.Go_AdHocDialog (closeToAdHocDialog, e) |> this.Broadcast

    member this.Go (dialog: 'ResultfulDialog) =
        fun (e: ReactEvent.Action) ->
            NavigationAction.Go_ResultfulDialog (dialog, e) |> this.Broadcast

    member this.Go (dialog: 'ResultlessDialog) =
        fun (e: ReactEvent.Action) ->
            NavigationAction.Go_ResultlessDialog (dialog, e) |> this.Broadcast

    member this.Go (dialog: SystemDialog) =
        fun (e: ReactEvent.Action) ->
            NavigationAction.Go_SystemDialog (dialog, e) |> this.Broadcast

    member this.Close (token: OpenDialogToken) (method: DialogCloseMethod) =
        fun (e: ReactEvent.Action) ->
            NavigationAction.Close (token, method, e) |> this.Broadcast


type NavigationImplementation<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality>(spec: LibRouter.RoutesSpec.Conversions<'Route, 'ResultlessDialog>, navigationState: NavigationState<'Route, 'ResultlessDialog, 'ResultfulDialog>, navigate: Router.Navigate, location: Router.Location, ?onNavigation: Location->bool) =

    let goUrlInNewTab (url: string) : unit =
        if ReactXP.Runtime.isWeb() then
            Browser.Dom.window?``open``(url, "_blank")
        else
            ReactXP.Linking.openUrl url

    let makeExternalUrl (appUrlBase: string) (location: Location) : string =
        appUrlBase + location.ToString

    member this.ProcessAction (action: NavigationAction<'Route, 'ResultlessDialog, 'ResultfulDialog>) : unit =
        match action with
        | NavigationAction.GoInSameTab_ResultlessDialog dialog                                  -> this.GoInSameTab dialog
        | NavigationAction.GoInSameTab_ResultfulDialog  dialog                                  -> this.GoInSameTab dialog
        | NavigationAction.GoInSameTab_SystemDialog     dialog                                  -> this.GoInSameTab dialog
        | NavigationAction.GoInSameTab_Route            route                                   -> this.GoInSameTab route
        | NavigationAction.GoInSameTab_Location         location                                -> this.GoInSameTab location
        | NavigationAction.GoInSameTab_Frame            frame                                   -> this.GoInSameTab frame
        | NavigationAction.GoInSameTab_Url              url                                     -> this.GoInSameTab url
        | NavigationAction.Redirect                     route                                   -> this.Redirect route
        | NavigationAction.GoInNewTab                   route                                   -> this.GoInNewTab route
        | NavigationAction.GoExternal                   url                                     -> this.GoExternal url
        | NavigationAction.GoExternalInSameTab          url                                     -> this.GoExternalInSameTab url
        | NavigationAction.Go                           (route, e)                              -> this.Go route e
        | NavigationAction.Replace                      (route, e)                              -> this.Replace route e
        | NavigationAction.ReplaceInSameTab             route                                   -> this.ReplaceInSameTab route
        | NavigationAction.GoExternalMaybeInNewTab      (url, e)                                -> this.GoExternalMaybeInNewTab url e
        | NavigationAction.Go_AdHocDialog               (closeToAdHocDialog, e)                 -> this.Go closeToAdHocDialog e
        | NavigationAction.Go_ResultfulDialog           (dialog, e)                             -> this.Go dialog e
        | NavigationAction.Go_ResultlessDialog          (dialog, e)                             -> this.Go dialog e
        | NavigationAction.Go_SystemDialog              (dialog, e)                             -> this.Go dialog e
        | NavigationAction.Go_Frame                     (frame, e)                              -> this.Go frame e
        | NavigationAction.Close                        (openDialogToken, dialogCloseMethod, e) -> this.Close openDialogToken dialogCloseMethod e
        | NavigationAction.Reload                                                               -> this.Reload ()
        | NavigationAction.GoBack                                                               -> this.GoBack ()

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

    member this.GoInSameTab (dialog: SystemDialog) : unit =
        match this.CurrentFrame with
        | Some currFrame ->
            let token = navigationState.DialogsState.AddSystem dialog
            this.GoInSameTab { currFrame with Dialogs = (NavigationDialog.System token) :: currFrame.Dialogs }
        | None -> failwith "Cannot open a dialog when we don't have a background route"

    member this.Redirect (route: 'Route) : unit =
        route
        |> NavigationFrame.ofRoute
        |> this.Redirect

    member _.Redirect (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : unit =
        navigate.Replace (spec.ToLocation navigationFrame).ToString

    member this.GoInSameTab (route: 'Route) : unit =
        route
        |> NavigationFrame.ofRoute
        |> this.GoInSameTab

    member _.GoInSameTab (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : unit =
        navigate.Push (spec.ToLocation navigationFrame).ToString

    member this.GoInSameTab (location: Location) : unit =
        let maybeNavigationFrame = location |> spec.FromLocation

        match maybeNavigationFrame with
        | Some navigationFrame -> this.GoInSameTab navigationFrame
        | None                 -> this.GoExternalInSameTab (makeExternalUrl spec.AppBaseUrl location)

    member this.GoInSameTab (url: string) : unit =
        match url |> Url.tryParse, spec.AppBaseUrl |> Url.tryParse with
        | Some parsedUrl, Some parsedAppBaseUrl when parsedUrl.Host <> parsedAppBaseUrl.Host ->
            this.GoExternalInSameTab url
        | Some parsedUrl, _ ->
            let maybeNavigationFrame =
                { Path = parsedUrl.Path; Query = parsedUrl.Fragment }
                |> spec.FromLocation

            match maybeNavigationFrame with
            | Some navigationFrame -> this.GoInSameTab navigationFrame
            | None                 -> this.GoExternalInSameTab url
        | _ -> ()

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
        navigate.GoBack ()

    member this.Go (route: 'Route) : ReactEvent.Action -> unit =
        route
        |> NavigationFrame.ofRoute
        |> this.Go

    member this.Go (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : ReactEvent.Action -> unit =
        fun (e: ReactEvent.Action) ->
            let newLocation = spec.ToLocation navigationFrame
            
            let continueNavigate = 
                onNavigation
                |> Option.map (fun onNav -> onNav newLocation)
                |> Option.defaultValue true
                
            if continueNavigate then
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
                    navigate.Push newLocation.ToString

    member this.Replace (route: 'Route) : ReactEvent.Action -> unit =
        route
        |> NavigationFrame.ofRoute
        |> this.Replace

    member this.ReplaceInSameTab (route: 'Route) : unit =
        route
        |> NavigationFrame.ofRoute
        |> this.ReplaceInSameTab

    member this.ReplaceInSameTab (navigationFrame: NavigationFrame<'Route, 'ResultlessDialog>) : unit =
        navigationState.Navigate NavigationDirection.Replace this.CurrentFrame
        // we reset because possibly not all routes set the page title,
        // which would cause the old title mismatching the new page to linger
        services().PageTitle.ResetRouteName()
        let newLocation = spec.ToLocation navigationFrame
        navigate.Replace newLocation.ToString

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
                navigate.Replace newLocation.ToString

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

    member this.Go (dialog: SystemDialog) : ReactEvent.Action -> unit =
        // NOTE very important to have the wrapper
        fun e ->
            let token = navigationState.DialogsState.AddSystem dialog
            this.GoToDialog (NavigationDialog.System token) e

    member this.Go (dialog: 'ResultlessDialog) : ReactEvent.Action -> unit =
        // NOTE very important to have the wrapper
        fun e ->
            let token = navigationState.DialogsState.AddResultless ()
            this.GoToDialog (NavigationDialog.Resultless (token, dialog)) e

    member private _.CurrentFrame : Option<NavigationFrame<'Route, 'ResultlessDialog>> =
        spec.FromLocation { Path = location.pathname; Query = location.search }

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
