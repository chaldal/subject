module LibClient.Components.AppShell.TopLevelErrorMessage

open LibClient
open Fable.Core

[<Emit("window.location.reload()")>]
let jsWindowLocationReload (): unit = jsNative

type Props = (* GenerateMakeFunction *) {
    Error: System.Exception
    Retry: unit -> unit
    key: string option // defaultWithAutoWrap JsUndefined
}

type TopLevelErrorMessage(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, TopLevelErrorMessage>("LibClient.Components.AppShell.TopLevelErrorMessage", _initialProps, Actions, hasStyles = true)

and Actions(this: TopLevelErrorMessage) =
    member _.reload(_) =
        if ReactXP.Runtime.isWeb() then
            jsWindowLocationReload()
        else 
            this.props.Retry()


let Make = makeConstructor<TopLevelErrorMessage, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
