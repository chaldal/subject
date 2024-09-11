module LibRouter.Components.NativeBackButton

open Fable.Core.JsInterop
open LibClient

let private BackHandler: obj = import "BackHandler" "react-native"

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap LibClient.JsInterop.Undefined
    goBack: unit -> unit
}

type NativeBackButton(initialProps) =
    inherit PureStatelessComponent<Props, Actions, NativeBackButton>("LibRouter.Components.NativeBackButton", initialProps, NoActions, hasStyles = false)

    override this.ComponentDidMount() : unit =
        // TODO - only go back if there is history available
        // Otherwise either fall back to default behaviour to allow exit app
        BackHandler?addEventListener ("hardwareBackPress",
            ( fun () ->
                this.props.goBack ()
                true
            )
        )

and Actions = unit

let Make = makeConstructor<NativeBackButton, _, _>

// Unfortunately necessary boilerplate
type Estate  = NoEstate
type Pstate  = NoPstate
