[<AutoOpen>]
module LibAppUpdateManager

open Fable.React
open LibClient
open LibClient.Services.LocalStorageService
open LibRouter.RoutesSpec
open LibClient.Components
open ReactXP.Components
open ReactXP.Styles
open LibAppUpdateManager.Components

module private Styles =
    let error = makeViewStyles {
        AlignSelf.Center
        flex      1
        padding   0
        minHeight 600
    }
    

type UpdateManager =
    [<Component>]
    static member Wrapper ( storageService: LocalStorageService, ``with``: (Option<Location>) -> (Option<Location->bool>) -> ReactElement, ?binaryUpdateUrl: NonemptyString) : ReactElement =
#if !EGGSHELL_PLATFORM_IS_WEB
        element {
            LC.ErrorBoundary (
            ``try`` = (
                CodePushUpdateManager.Wrapper (storageService, ``with``, ?binaryUpdateUrl = binaryUpdateUrl)
            ),
            catch =
                (fun (error, retry) ->
                    RX.View (styles = [|Styles.error|], children = [|
                        CodePushUpdateManager.FallBackWrapper ()
                        LC.AppShell.TopLevelErrorMessage (retry = retry, error = error)
                    |])
                )
            )
        }
#else
        element {
            LC.ErrorBoundary (
            ``try`` = (
                ``with`` None None
            ),
            catch =
                (fun (error, retry) ->
                    RX.View (styles = [|Styles.error|], children = [|
                        LC.AppShell.TopLevelErrorMessage (retry = retry, error = error)
                    |])
                )
            )
        }
#endif