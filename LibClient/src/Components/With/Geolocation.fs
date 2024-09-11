[<AutoOpen>]
module LibClient.Components.With_Geolocation

open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open LibClient
open LibClient.Components

module LC =
    module With =
        module Geolocation =
            type LatLng = float * float

open LC.With.Geolocation

type LC.With with
    [<Component>]
    static member Geolocation (``with``: Option<LatLng> -> ReactElement) : ReactElement =
        let latLngState = Hooks.useState<Option<LatLng>> None

        Hooks.useEffect(
            fun () ->
                async {
                    let! latLngResult = ReactXP.Helpers.ReactXPRaw?Location?getCurrentPosition({||}) |> Async.AwaitPromise |> Async.TryCatch

                    let latLng =
                        match latLngResult with
                        | Ok latLng ->
                            Some (latLng?coords?latitude, latLng?coords?longitude)
                        | Error _   ->
                            None

                    latLngState.update latLng
                } |> startSafely
        )

        ``with`` latLngState.current