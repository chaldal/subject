module LibRouter.Components.Link

open Fable.Core.JsInterop

type Props = (* GenerateMakeFunction *) {
    To:  string
    key: string option // defaultWithAutoWrap LibClient.JsInterop.Undefined
}

let Link: obj = Fable.Core.JsInterop.import "Link" "react-router-dom"

let Make =
    LibClient.ThirdParty.wrapComponentTransformingProps<Props>
        Link
        (fun (props: Props) ->
            createObj [
                "to"       ==> props.To
                "children" ==> props?children
            ]
        )
