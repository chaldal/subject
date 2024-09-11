namespace LibRouter.Components

open LibClient
open LibClient.Dialogs
open LibRouter.RoutesSpec
open LibRouter.Components.With.Navigation
open LibRouter.Components.Dialogs
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module DialogsTypeExtensions =
    type LibRouter.Components.Constructors.LR with
        static member Dialogs(nav: Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog>, dialogs: List<NavigationDialog<'ResultlessDialog>>, dialogsState: DialogsState<'ResultfulDialog>, makeResultless: ('ResultlessDialog * (LibClient.Dialogs.DialogCloseMethod -> ReactEvent.Action -> unit)) -> ReactElement, makeResultful: ('ResultfulDialog  * (LibClient.Dialogs.DialogCloseMethod -> ReactEvent.Action -> unit)) -> ReactElement, ?children: ReactChildrenProp, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Nav = nav
                    Dialogs = dialogs
                    DialogsState = dialogsState
                    MakeResultless = makeResultless
                    MakeResultful = makeResultful
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibRouter.Components.Dialogs.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            