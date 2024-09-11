namespace LibClient.Components

open LibClient
open Fable.React
open LibClient.Components.AppShell.Content
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module AppShell_ContentTypeExtensions =
    type LibClient.Components.Constructors.LC.AppShell with
        static member Content(desktopSidebarStyle: DesktopSidebarStyle, sidebar: ReactElement, content: ReactElement, dialogs: ReactElement, onError: (System.Exception * (unit -> unit)) -> ReactElement, ?children: ReactChildrenProp, ?topStatus: ReactElement, ?topNav: ReactElement, ?bottomNav: ReactElement, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    DesktopSidebarStyle = desktopSidebarStyle
                    Sidebar = sidebar
                    Content = content
                    Dialogs = dialogs
                    OnError = onError
                    TopStatus = defaultArg topStatus (noElement)
                    TopNav = defaultArg topNav (noElement)
                    BottomNav = defaultArg bottomNav (noElement)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.AppShell.Content.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            