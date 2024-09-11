module ThirdParty.FacebookPixel.Native

open Fable.Core.JsInterop
open LibClient.Json

let private AppEventsLogger: obj = import "AppEventsLogger"  "react-native-fbsdk";
let private DefaultCurrency: string = "BDT"

let private trackEvent (eventName: string) (price: UnsignedDecimal) (properties: obj) : unit =
    AppEventsLogger?logEvent eventName (float price.Value) properties

let CompleteRegistration (): unit =
    AppEventsLogger?logEvent "fb_mobile_complete_registration"

let InitiateCheckout (contentIds: List<string>) (price: UnsignedDecimal) : unit =
    let properties =
        {|
            fb_content_type = "product"
            fb_content_id   = contentIds |> List.toArray |> Json.ToString
            fb_currency     = DefaultCurrency
            fb_num_items    = contentIds |> List.length
        |}
    trackEvent "fb_mobile_initiated_checkout" price properties

let ViewContent (contentId: string) (price: UnsignedDecimal) : unit =
    let properties =
        {|
            fb_content_type = "product"
            fb_content_id   = contentId
            fb_currency     = DefaultCurrency
        |}
    trackEvent "fb_mobile_content_view" price properties

let Purchase (contentsWithIdAndQuantity: List<string * int>) (price: UnsignedDecimal) : unit =
    let contentsFormatted =
        contentsWithIdAndQuantity
        |> List.map (fun (id, quantity) -> {| id = id; quantity = quantity |})
        |> List.toArray
        |> Json.ToString

    let properties =
        {|
            fb_content      = contentsFormatted
            fb_content_type = "product"
        |}
    AppEventsLogger?logPurchase (float price.Value) DefaultCurrency properties

let AddToCart (contentWithIdAndQuantity: string * int) (price: UnsignedDecimal) : unit =
    let properties =
        {|
            fb_content_type = "product"
            fb_content      = contentWithIdAndQuantity |> fun (id, quantity) -> [| {| id = id; quantity = quantity |} |] |> Json.ToString
            fb_currency     = DefaultCurrency
        |}
    trackEvent "fb_mobile_add_to_cart" price properties