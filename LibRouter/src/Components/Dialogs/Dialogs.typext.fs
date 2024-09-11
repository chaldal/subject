module LibRouter.Components.Dialogs

open LibClient
open LibClient.Dialogs
open LibRouter.RoutesSpec
open LibRouter.Components.With.Navigation

type Props<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality> = (* GenerateMakeFunction *) {
    Nav:            Navigation<'Route, 'ResultlessDialog, 'ResultfulDialog>
    Dialogs:        List<NavigationDialog<'ResultlessDialog>>
    DialogsState:   DialogsState<'ResultfulDialog>
    MakeResultless: ('ResultlessDialog * (LibClient.Dialogs.DialogCloseMethod -> ReactEvent.Action -> unit)) -> ReactElement
    MakeResultful:  ('ResultfulDialog  * (LibClient.Dialogs.DialogCloseMethod -> ReactEvent.Action -> unit)) -> ReactElement
    key: string option // defaultWithAutoWrap JsUndefined
}

type Dialogs<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality>(initialProps) =
    inherit PureStatelessComponent<Props<'Route, 'ResultlessDialog, 'ResultfulDialog>, Actions<'Route, 'ResultlessDialog, 'ResultfulDialog>, Dialogs<'Route, 'ResultlessDialog, 'ResultfulDialog>>("LibRouter.Components.Dialogs", initialProps, Actions<'Route, 'ResultlessDialog, 'ResultfulDialog>, hasStyles = true)

    do
        LibClient.Dialogs.AdHoc.provideGoImplementation (fun (closeToDialog: ((DialogCloseMethod -> ReactEvent.Action -> unit) -> ReactElement)) ->
            initialProps.Nav.Go closeToDialog
        )

and Actions<'Route, 'ResultlessDialog, 'ResultfulDialog when 'Route: equality>(_this: Dialogs<'Route, 'ResultlessDialog, 'ResultfulDialog>) =
    class end

let Make = makeConstructor<Dialogs<'Route, 'ResultlessDialog, 'ResultfulDialog>, _, _>

type Estate<'Route, 'ResultlessDialog, 'ResultfulDialog> = NoEstate3<'Route, 'ResultlessDialog, 'ResultfulDialog>
type Pstate = NoPstate
