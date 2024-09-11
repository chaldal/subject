module AppEggShellGallery.Components.Content.ThirdParty.Map

open LibClient
open ThirdParty.Map

let customStyles =
    [
        {
            FeatureType = MapFeatureType.Landscape
            ElementType = MapElementType.All
            Stylers =
                [|
                    MapStyler.Color "#d0d3e7"
                |]
        }
        {
            FeatureType = MapFeatureType.PointsOfInterest
            ElementType = MapElementType.GeometryFill
            Stylers =
                [|
                    MapStyler.Color "#c6c9cd"
                |]
        }
        {
            FeatureType = MapFeatureType.PointsOfInterest
            ElementType = MapElementType.LabelsIcon
            Stylers =
                [|
                    MapStyler.Color "#9298a0"
                |]
        }
        {
            FeatureType = MapFeatureType.PointsOfInterest
            ElementType = MapElementType.LabelsTextFill
            Stylers =
                [|
                    MapStyler.Color "#6c717a"
                |]
        }
        {
            FeatureType = MapFeatureType.PointsOfInterestBusiness
            ElementType = MapElementType.GeometryFill
            Stylers =
                [|
                    MapStyler.Color "#c6c9cd"
                |]
        }
        {
            FeatureType = MapFeatureType.Road
            ElementType = MapElementType.All
            Stylers =
                [|
                    MapStyler.Color "#fafafa"
                |]
        }
        {
            FeatureType = MapFeatureType.Transit
            ElementType = MapElementType.GeometryFill
            Stylers =
                [|
                    MapStyler.Color "#9298a0"
                |]
        }
        {
            FeatureType = MapFeatureType.Transit
            ElementType = MapElementType.LabelsIcon
            Stylers =
                [|
                    MapStyler.Color "#b0b4ba"
                |]
        }
        {
            FeatureType = MapFeatureType.Water
            ElementType = MapElementType.All
            Stylers =
                [|
                    MapStyler.Color "#9cc1f2"
                |]
        }
    ]

let getMarkerWithInfoWindow () =
    Marker.init MarkerPosition.Centered
    |> Marker.withInfoWindow
        (
            InfoWindow.init (
                fun infoWindowHandle ->
                    AppEggShellGallery.Components.Content.ThirdParty.ToolWindowContent.Make {key=None; Handle = infoWindowHandle} [||]
                )
        )

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, ThirdParty.Map.Types.MapPosition>
}

type Map(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Map>("AppEggShellGallery.Components.Content.ThirdParty.Map", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        Values = Map.ofList [("a", MapPosition.LatLng (34.8524359, 135.4587936))]
    }

and Actions(this: Map) =
    member _.OnPositionChanged (key: string) (position: ThirdParty.Map.Types.MapPosition) : unit =
        this.SetEstate (fun estate _ ->
            { estate with Values = estate.Values.AddOrUpdate(key, position) }
        )

let Make = makeConstructor<Map, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
