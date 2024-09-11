module ThirdParty.Map.Components.Native.MapRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.Map.Components

open LibClient
open LibClient.RenderHelpers
open ThirdParty.Map.LocalImages

open ThirdParty.Map.Components.Native.Map
open ThirdParty.Map.Types


let render(children: array<ReactElement>, props: ThirdParty.Map.Components.Native.Map.Props, estate: ThirdParty.Map.Components.Native.Map.Estate, pstate: ThirdParty.Map.Components.Native.Map.Pstate, actions: ThirdParty.Map.Components.Native.Map.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "LibClient.Components.With.Layout"
    LibClient.Components.Constructors.LC.With.Layout(
        ``with`` =
            (fun (onLayoutOption, maybeLayout) ->
                    (castAsElementAckingKeysWarning [|
                        let __parentFQN = Some "ReactXP.Components.View"
                        let __currClass = (System.String.Format("{0}", TopLevelBlockClass)) + System.String.Format(" {0} {1}", (if (props.FullScreen) then "full-screen" else ""), (if (not props.FullScreen) then "view" else ""))
                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                        ReactXP.Components.Constructors.RX.View(
                            ?onLayout = (onLayoutOption),
                            ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                            children =
                                [|
                                    let __parentFQN = Some "ThirdParty.Map.Components.Native.ReactNativeMaps"
                                    ThirdParty.Map.Components.Constructors.Map.Native.ReactNativeMaps(
                                        ref = (props.Ref),
                                        onChange = (props.OnChange),
                                        value = (match props.Value with Some latlng -> latlng | _ -> defaultLatLng),
                                        zoom = (props.Zoom),
                                        size = (maybeLayout |> Option.map (fun l -> l.Width, l.Height)),
                                        children =
                                            [|
                                                (
                                                    (props.Markers)
                                                    |> Option.map
                                                        (fun markers ->
                                                            (castAsElementAckingKeysWarning [|
                                                                (
                                                                    (markers)
                                                                    |> Seq.map
                                                                        (fun marker ->
                                                                            (castAsElementAckingKeysWarning [|
                                                                                match (marker.Position) with
                                                                                | MarkerPosition.LatLng _ ->
                                                                                    [|
                                                                                        (
                                                                                            (markers)
                                                                                            |> Seq.map
                                                                                                (fun marker ->
                                                                                                    let __parentFQN = Some "ThirdParty.Map.Components.Native.Marker"
                                                                                                    ThirdParty.Map.Components.Constructors.Map.Native.Marker(
                                                                                                        image = (getMarkerImage marker),
                                                                                                        draggable = (false),
                                                                                                        coordinate = (getCoordinate marker)
                                                                                                    )
                                                                                                )
                                                                                            |> Array.ofSeq |> castAsElement
                                                                                        )
                                                                                    |]
                                                                                | _ ->
                                                                                    [||]
                                                                                    (*  NOOP ` *)
                                                                                |> castAsElementAckingKeysWarning
                                                                            |])
                                                                        )
                                                                    |> Array.ofSeq |> castAsElement
                                                                )
                                                            |])
                                                        )
                                                    |> Option.getOrElse noElement
                                                )
                                                (
                                                    (props.Shapes)
                                                    |> Option.map
                                                        (fun shapes ->
                                                            (castAsElementAckingKeysWarning [|
                                                                (
                                                                    (shapes)
                                                                    |> Seq.map
                                                                        (fun shape ->
                                                                            (castAsElementAckingKeysWarning [|
                                                                                match (shape) with
                                                                                | Shape.Polyline polyline ->
                                                                                    [|
                                                                                        let __parentFQN = Some "ThirdParty.Map.Components.Native.Polyline"
                                                                                        ThirdParty.Map.Components.Constructors.Map.Native.Polyline(
                                                                                            value = (polyline)
                                                                                        )
                                                                                    |]
                                                                                | Shape.Circle circle ->
                                                                                    [|
                                                                                        let __parentFQN = Some "ThirdParty.Map.Components.Native.Circle"
                                                                                        ThirdParty.Map.Components.Constructors.Map.Native.Circle(
                                                                                            circle = (circle)
                                                                                        )
                                                                                    |]
                                                                                | Shape.Polygon _polygon ->
                                                                                    [||]
                                                                                    (*  TODO Implement this  *)
                                                                                |> castAsElementAckingKeysWarning
                                                                            |])
                                                                        )
                                                                    |> Array.ofSeq |> castAsElement
                                                                )
                                                            |])
                                                        )
                                                    |> Option.getOrElse noElement
                                                )
                                            |]
                                    )
                                    (*
        This was an hack implemented by previous developers
        Added a image marker and manually center this with styling.
        This is already in production and apps use this
        If I change this now, they need to change the Implementation from their side as well.

        So keeping this for now
         *)
                                    (
                                        (props.Markers)
                                        |> Option.map
                                            (fun markers ->
                                                (castAsElementAckingKeysWarning [|
                                                    (
                                                        (markers)
                                                        |> Seq.map
                                                            (fun marker ->
                                                                (castAsElementAckingKeysWarning [|
                                                                    match (marker.Position) with
                                                                    | MarkerPosition.Centered ->
                                                                        [|
                                                                            let __parentFQN = Some "ReactXP.Components.Image"
                                                                            let __currClass = "image"
                                                                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                                            ReactXP.Components.Constructors.RX.Image(
                                                                                size = (ReactXP.Components.Image.FromStyles),
                                                                                source = (localImage "/libs/ThirdParty/Map/images/marker.png"),
                                                                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.Image" __currStyles |> Some) else None)
                                                                            )
                                                                        |]
                                                                    | _ ->
                                                                        [||]
                                                                        (*  NOOP ` *)
                                                                    |> castAsElementAckingKeysWarning
                                                                |])
                                                            )
                                                        |> Array.ofSeq |> castAsElement
                                                    )
                                                |])
                                            )
                                        |> Option.getOrElse noElement
                                    )
                                |]
                        )
                    |])
            )
    )
