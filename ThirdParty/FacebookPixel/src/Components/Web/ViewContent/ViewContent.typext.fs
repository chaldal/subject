module ThirdParty.FacebookPixel.Components.Web.ViewContent

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
    Id:  string
    Price: UnsignedDecimal
}

type ViewContent(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ViewContent>("ThirdParty.FacebookPixel.Components.Web.ViewContent", _initialProps, Actions, hasStyles = false)

    override this.ComponentDidMount() : unit =
        ThirdParty.FacebookPixel.Base.TrackViewContent this.props.Id this.props.Price

and Actions(_this: ViewContent) =
    class end

let Make = makeConstructor<ViewContent, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
