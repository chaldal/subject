module ThirdParty.GoogleAnalytics.Native

open Fable.Core
open Fable.Core.JsInterop
open LibClient

let Analytics (_screenViewParam: obj): obj  = importDefault "@react-native-firebase/analytics"

let private setAnalyticsUserForTelemetryUser (user: TelemetryUser) : unit =
    match user with
    | TelemetryUser.Identified (userId, _) ->
        Analytics()?setUserId userId
    | TelemetryUser.Anonymous ->
        Analytics()?setUserId null

let TrackViewItem (itemId: string) (itemName: string) (price: decimal) (currency: string) =
    Telemetry.GetUser() |> setAnalyticsUserForTelemetryUser
    let item = createObj [
        "item_id"   ==> itemId
        "item_name" ==> itemName
        "price"     ==> float price
    ]
    let viewParam = createObj [
        "currency" ==> currency
        "value"    ==> float price
        "items"    ==> [| item |]
    ]
    Analytics()?logViewItem viewParam

let TrackAddToCart (itemId: string) (itemName: string) (quantity: int) (price: decimal) (currency: string) =
    Telemetry.GetUser() |> setAnalyticsUserForTelemetryUser
    let item = createObj [
        "item_id"   ==> itemId
        "item_name" ==> itemName
        "price"     ==> float price
        "quantity"  ==> quantity
    ]
    let cartParam = createObj [
        "currency" ==> currency
        "value"    ==> float price * float quantity
        "items"    ==> [| item |]
    ]
    Analytics()?logAddToCart cartParam

let TrackBeginCheckout (items: List<Types.FirebaseItem>) (orderTotal: decimal) (currency: string) =
    Telemetry.GetUser() |> setAnalyticsUserForTelemetryUser
    let itemsInLibraryFormat : obj[] = items |> List.map (fun item -> item.toLibraryObj()) |> Array.ofList
    let beginCheckoutParam = createObj [
        "currency"       ==> currency
        "value"          ==> float orderTotal
        "items"          ==> itemsInLibraryFormat
    ]
    Analytics()?logBeginCheckout beginCheckoutParam

let TrackPurchase (orderId: string) (items: List<Types.FirebaseItem>) (orderTotal: decimal) (shippingCost: decimal) (currency: string) =
    Telemetry.GetUser() |> setAnalyticsUserForTelemetryUser
    let itemsInLibraryFormat: obj[] = items |> List.map (fun item -> item.toLibraryObj()) |> Array.ofList
    let purchaseParam = createObj [
        "transaction_id" ==> orderId
        "currency"       ==> currency
        "value"          ==> float orderTotal
        "shipping"       ==> float shippingCost
        "items"          ==> itemsInLibraryFormat
    ]
    Analytics()?logPurchase purchaseParam

type GoogleAnalyticsTelemetrySink () =
    interface ITelemetrySink with
        member _.TrackEvent (_: string) (_: TelemetryUser) (_: TelemetryProperties) : unit = ()

        member _.TrackScreenView (url: string) (user: TelemetryUser) (_: TelemetryProperties) : unit =
            setAnalyticsUserForTelemetryUser user

            Analytics()?logScreenView (createObj [
            "screen_name" ==> url
            ])
            |> Async.AwaitPromise
            |> startSafely