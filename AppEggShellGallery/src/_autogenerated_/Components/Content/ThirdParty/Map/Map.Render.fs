module AppEggShellGallery.Components.Content.ThirdParty.MapRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open LibRouter.Components
open ThirdParty.Map.Components
open ReactXP.Components
open ThirdParty.Recharts.Components
open ThirdParty.Showdown.Components
open ThirdParty.SyntaxHighlighter.Components
open LibUiAdmin.Components
open AppEggShellGallery.Components

open LibLang
open LibClient
open LibClient.Services.Subscription
open LibClient.RenderHelpers
open LibClient.Chars
open LibClient.ColorModule
open LibClient.Responsive
open AppEggShellGallery.RenderHelpers
open AppEggShellGallery.Navigation
open AppEggShellGallery.LocalImages
open AppEggShellGallery.Icons
open AppEggShellGallery.AppServices
open AppEggShellGallery

open AppEggShellGallery.Components.Content.ThirdParty.Map
open ThirdParty.Map


let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.ThirdParty.Map.Props, estate: AppEggShellGallery.Components.Content.ThirdParty.Map.Estate, pstate: AppEggShellGallery.Components.Content.ThirdParty.Map.Pstate, actions: AppEggShellGallery.Components.Content.ThirdParty.Map.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "ThirdParty.Map.Components.Base"),
        displayName = ("Map"),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Basics"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'
                        Position='estate.Values.TryFind ""a"" |> Option.defaultValue (MapPosition.LatLng (0, 0))'
                        OnPositionChanged='actions.OnPositionChanged ""a""'/>

                    <div>
                        Position: {estate.Values.TryFind ""a""}
                    </div>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        onPositionChanged = (actions.OnPositionChanged "a"),
                                                        position = (estate.Values.TryFind "a" |> Option.defaultValue (MapPosition.LatLng (0, 0))),
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                    let __parentFQN = Some "ReactXP.Components.View"
                                                    ReactXP.Components.Constructors.RX.View(
                                                        children =
                                                            [|
                                                                makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "Position: ", (estate.Values.TryFind "a"), ""))
                                                            |]
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Markers"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'
                        Position='(-27.1687243,152.9308381) |> MapPosition.LatLng'
                        Markers='[ Marker.init MarkerPosition.Centered |> Marker.withTooltip ""Centered Marker"" ]'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        markers = ([ Marker.init MarkerPosition.Centered |> Marker.withTooltip "Centered Marker" ]),
                                                        position = ((-27.1687243,152.9308381) |> MapPosition.LatLng),
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'
                        Markers='[ MarkerPosition.LatLng (-27.1687243,152.9308381) |> Marker.init |> Marker.withLabel (MarkerLabel.init ""A""); MarkerPosition.LatLng (-27.2,153.0) |> Marker.init |> Marker.withLabel (MarkerLabel.init ""B"") ]'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        markers = ([ MarkerPosition.LatLng (-27.1687243,152.9308381) |> Marker.init |> Marker.withLabel (MarkerLabel.init "A"); MarkerPosition.LatLng (-27.2,153.0) |> Marker.init |> Marker.withLabel (MarkerLabel.init "B") ]),
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'
                        Markers='[ MarkerPosition.LatLng (-27.1687243,152.9308381) |> Marker.init |> Marker.withLabel (MarkerLabel.init ""A"") |> Marker.withImage (Icon.init ""https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png"" |> Icon.withAnchor (0, 32) |> ~Icon); MarkerPosition.LatLng (-27.2,153.0) |> Marker.init |> Marker.withLabel (MarkerLabel.init ""B"") |> Marker.withImage (Symbol.init ""M10.453 14.016l6.563-6.609-1.406-1.406-5.156 5.203-2.063-2.109-1.406 1.406zM12 2.016q2.906 0 4.945 2.039t2.039 4.945q0 1.453-0.727 3.328t-1.758 3.516-2.039 3.070-1.711 2.273l-0.75 0.797q-0.281-0.328-0.75-0.867t-1.688-2.156-2.133-3.141-1.664-3.445-0.75-3.375q0-2.906 2.039-4.945t4.945-2.039z"" |> Symbol.withAnchor (15, 30) |> Symbol.withFillColor ""blue"" |> Symbol.withFillOpacity 0.8 |> Symbol.withStrokeColor ""red"" |> Symbol.withStrokeOpacity 0.5 |> ~Symbol) ]'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        markers = ([ MarkerPosition.LatLng (-27.1687243,152.9308381) |> Marker.init |> Marker.withLabel (MarkerLabel.init "A") |> Marker.withImage (Icon.init "https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png" |> Icon.withAnchor (0, 32) |> ThirdParty.Map.Components.Base.Icon); MarkerPosition.LatLng (-27.2,153.0) |> Marker.init |> Marker.withLabel (MarkerLabel.init "B") |> Marker.withImage (Symbol.init "M10.453 14.016l6.563-6.609-1.406-1.406-5.156 5.203-2.063-2.109-1.406 1.406zM12 2.016q2.906 0 4.945 2.039t2.039 4.945q0 1.453-0.727 3.328t-1.758 3.516-2.039 3.070-1.711 2.273l-0.75 0.797q-0.281-0.328-0.75-0.867t-1.688-2.156-2.133-3.141-1.664-3.445-0.75-3.375q0-2.906 2.039-4.945t4.945-2.039z" |> Symbol.withAnchor (15, 30) |> Symbol.withFillColor "blue" |> Symbol.withFillOpacity 0.8 |> Symbol.withStrokeColor "red" |> Symbol.withStrokeOpacity 0.5 |> ThirdParty.Map.Components.Base.Symbol) ]),
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Shapes"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'
                        Position='(-27.1687243,152.9308381) |> MapPosition.LatLng'
                        Shapes='[ [ (-27.1687243,152.9308381); (-27.1,153.1); (-27.1,152.8) ] |> Polyline.init |> Polyline.withStrokeColor ""#1000CF"" |> Polyline.withStrokeWeight 2 |> Polyline.withStrokeOpacity 0.75 |> ~Polyline ]'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        shapes = ([ [ (-27.1687243,152.9308381); (-27.1,153.1); (-27.1,152.8) ] |> Polyline.init |> Polyline.withStrokeColor "#1000CF" |> Polyline.withStrokeWeight 2 |> Polyline.withStrokeOpacity 0.75 |> ThirdParty.Map.Components.Base.Polyline ]),
                                                        position = ((-27.1687243,152.9308381) |> MapPosition.LatLng),
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'
                        Position='(-27.1687243,152.9308381) |> MapPosition.LatLng'
                        Shapes='[ [ seq { (-27.1687243,152.9308381); (-27.1,153.1); (-27.1,152.8) } ] |> Polygon.init |> Polygon.withStrokeColor ""#1000CF"" |> Polygon.withStrokeWeight 2 |> Polygon.withStrokeOpacity 0.75 |> ~Polygon ]'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        shapes = ([ [ seq { (-27.1687243,152.9308381); (-27.1,153.1); (-27.1,152.8) } ] |> Polygon.init |> Polygon.withStrokeColor "#1000CF" |> Polygon.withStrokeWeight 2 |> Polygon.withStrokeOpacity 0.75 |> ThirdParty.Map.Components.Base.Polygon ]),
                                                        position = ((-27.1687243,152.9308381) |> MapPosition.LatLng),
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'
                        Position='(-27.1687243,152.9308381) |> MapPosition.LatLng'
                        Shapes='[ Circle.init (~LatLng (-27.1687243,152.9308381)) 500 |> Circle.withFillColor ""#401020"" |> Circle.withFillOpacity 0.3 |> Circle.withStrokeColor ""#1000CF"" |> Circle.withStrokeWeight 2 |> Circle.withStrokeOpacity 0.75 |> Circle.withEditable true |> ~Circle ]'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        shapes = ([ Circle.init (ThirdParty.Map.Components.Base.LatLng (-27.1687243,152.9308381)) 500 |> Circle.withFillColor "#401020" |> Circle.withFillOpacity 0.3 |> Circle.withStrokeColor "#1000CF" |> Circle.withStrokeWeight 2 |> Circle.withStrokeOpacity 0.75 |> Circle.withEditable true |> ThirdParty.Map.Components.Base.Circle ]),
                                                        position = ((-27.1687243,152.9308381) |> MapPosition.LatLng),
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Info Windows"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'
                        Position='(-27.1687243,152.9308381) |> MapPosition.LatLng'
                        Markers='[ getMarkerWithInfoWindow () ]'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        markers = ([ getMarkerWithInfoWindow () ]),
                                                        position = ((-27.1687243,152.9308381) |> MapPosition.LatLng),
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Custom Styles"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <Map.Base
                        class='map'
                        ApiKey='AppEggShellGallery.Config.current().GoogleMapsApiKey'
                        Position='(-27.1687243,152.9308381) |> MapPosition.LatLng'
                        MapStyles='customStyles'/>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "ThirdParty.Map.Components.Base"
                                                    let __currClass = "map"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    ThirdParty.Map.Components.Constructors.Map.Base(
                                                        mapStyles = (customStyles),
                                                        position = ((-27.1687243,152.9308381) |> MapPosition.LatLng),
                                                        apiKey = (AppEggShellGallery.Config.current().GoogleMapsApiKey),
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )
