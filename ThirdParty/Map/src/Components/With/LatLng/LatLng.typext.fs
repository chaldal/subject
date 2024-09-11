module ThirdParty.Map.Components.With.LatLng

open LibClient

type LatLngType = ThirdParty.Map.Types.LatLng

type Props = (* GenerateMakeFunction *) {
    Address: string
    With:    AsyncData<LatLngType> -> ReactElement
    ApiKey:  string
    key: string option // defaultWithAutoWrap JsUndefined
}

type LatLng(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, LatLng>("ThirdParty.Map.Components.With.LatLng", _initialProps, Actions, hasStyles = false)

and Actions(_this: LatLng) =
    class end

let Make = makeConstructor<LatLng, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
