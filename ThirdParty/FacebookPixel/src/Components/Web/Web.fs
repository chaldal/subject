module ThirdParty.FacebookPixel.Web

open Fable.Core.JsInterop
open LibClient

let private FacebookPixelRaw: obj = importDefault "react-facebook-pixel"
let private DefaultCurrency: string = "BDT"

let private trackEvent (name: string) (properties: obj) : unit =
    FacebookPixelRaw?track name properties

let CompleteRegistration : unit =
    FacebookPixelRaw?track "CompleteRegistration"

let InitiateCheckout (contentIds: List<string>) (price: UnsignedDecimal) : unit =
    let properties =
        {|
            content_ids = contentIds |> List.toArray
            currency    = DefaultCurrency
            value       = float price.Value
            num_items   = contentIds |> List.length
        |}
    trackEvent "InitiateCheckout" properties

let ViewContent (contentId: string) (price: UnsignedDecimal) : unit =
    let properties =
        {|
            content_ids  = [|contentId|]
            currency     = DefaultCurrency
            value        = float price.Value
            content_type = "product"
        |}
    trackEvent "ViewContent" properties

let Purchase (contentIds: List<string>) (price: UnsignedDecimal) : unit =
    let properties =
        {|
            content_ids = contentIds |> List.toArray
            currency    = DefaultCurrency
            value       = float price.Value
            num_items   = contentIds |> List.length
        |}
    trackEvent "Purchase" properties

let AddToCart (contentWithIdAndQuantity: string * int) (price: UnsignedDecimal) : unit =
    let properties =
        {|
            content_ids  = [| (contentWithIdAndQuantity |> fun (id, _) -> id) |]
            content      = contentWithIdAndQuantity |> fun (id, quantity) -> [| {| id = id; quantity = quantity |} |]
            currency     = DefaultCurrency
            value        = float price.Value
        |}
    trackEvent "AddToCart" properties

type FacebookPixelTelemetrySink (FacebookPixelIdKey: string) =
    do FacebookPixelRaw?init FacebookPixelIdKey

    interface ITelemetrySink with
        member _.TrackEvent (_: string) (_: TelemetryUser) (_: TelemetryProperties) : unit = ()

        member _.TrackScreenView (_: string) (_: TelemetryUser) (_: TelemetryProperties) : unit =
            FacebookPixelRaw?pageView ()