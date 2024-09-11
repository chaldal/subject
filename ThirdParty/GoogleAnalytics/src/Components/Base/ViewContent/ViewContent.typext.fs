module ThirdParty.GoogleAnalytics.Components.Base.ViewContent

open LibClient

type Props = (* GenerateMakeFunction *) {
    key:      string option // defaultWithAutoWrap JsUndefined
    Id:       string
    Name:     string
    Price:    UnsignedDecimal
    Currency: string
}

type ViewContent(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ViewContent>("ThirdParty.GoogleAnalytics.Components.Base.ViewContent", _initialProps, Actions, hasStyles = false)

    override this.ComponentDidMount() : unit =
        ThirdParty.GoogleAnalytics.Base.TrackViewItem this.props.Id this.props.Name this.props.Price.Value this.props.Currency

and Actions(_this: ViewContent) =
    class end

let Make = makeConstructor<ViewContent, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate