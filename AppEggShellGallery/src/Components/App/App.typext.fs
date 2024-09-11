module AppEggShellGallery.Components.App

open LibLang
open LibClient
open LibRouter.RoutesSpec
open Fable.Core.JsInterop
open AppEggShellGallery.Navigation
open System.Text.RegularExpressions

type Props  = LibRouter.App.Props
type Estate = EmptyRecordType

let registerGlobalMarkdownLinkHandler (nav: AppEggShellGallery.Navigation.Navigation) : ReactElement =
    Browser.Dom.window?globalMarkdownLinkHandler <- (fun (e: Browser.Types.PointerEvent) (markdownUrl: string) ->
        let actionEvent = ReactEvent.Pointer.OfBrowserEvent e |> ReactEvent.Action.Make

        if Regex.IsMatch (markdownUrl, "^(http|https)://") then
            nav.GoExternalMaybeInNewTab markdownUrl actionEvent

        else if markdownUrl.StartsWith "gallery://act/" then
            match markdownUrl.Substring("gallery://act/".Length) with
            | "toggleTapCaptureDebugVisualization" ->
                LibClient.Components.TapCaptureDebugVisibility.toggleVisibleForDebug ()
            | _ -> Noop

        else if markdownUrl.StartsWith "gallery://" then
            let encodedRoute = markdownUrl.Substring("gallery://".Length)
            match routesSpec().FromLocation (Location.ofPath encodedRoute) with
            // NOTE this None only catches urls that match no pattern in the Navigation route spec,
            // but if there's an encoding error, it just gets dumped on the console, and to change
            // that would be a lot of work (follow GetFromJson in RoutesSpec to see)
            | None       -> Browser.Dom.window.alert ("Could not decode in-gallery URL link from a markdown document. Please let the EggShell team know. " + encodedRoute)
            | Some frame -> nav.Go frame actionEvent

        else
            let route =
                if markdownUrl.StartsWith "./" then
                    let trimmedUrl = markdownUrl.Substring("./".Length)
                    if markdownUrl.StartsWith "./tools/" then
                        Tools trimmedUrl
                    else if markdownUrl.StartsWith "./how-to/" then
                        HowTo (HowToItem.Markdown trimmedUrl)
                    else
                        Docs trimmedUrl
                else
                    Docs markdownUrl

            nav.Go (None, route) actionEvent
    )
    noElement

type App(initialProps: Props) =
    inherit LibRouter.App.AppComponent<Props, Estate, Actions, App>("AppEggShellGallery.Components.App", initialProps, Actions, hasStyles = true)

and Actions(_this: App) =
    class end

let Make = makeConstructor<App,_,_>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
