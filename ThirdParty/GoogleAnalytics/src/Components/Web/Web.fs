module ThirdParty.GoogleAnalytics.Web

open Fable.Core.JsInterop
open LibClient

let private initializeApp: obj -> obj  = import "initializeApp" "firebase/app"
let private getAnalytics:  obj -> obj  = import "getAnalytics" "firebase/analytics"

let private logEvent  (_analytics: obj, _eventName: string, _properties: obj): unit  = import "logEvent" "firebase/analytics"
let private setUserId (_analytics: obj, _userId: string): unit                       = import "setUserId" "firebase/analytics"

let mutable private maybeAnalytics : Option<obj> = None

let private initialize (app: obj)=
    maybeAnalytics <- Some (getAnalytics app)

let TrackViewItem (itemId: string) (itemName: string) (price: decimal) (currency: string) =
    maybeAnalytics
    |> Option.sideEffect (fun analytics ->
        let item = createObj [
            "item_id"   ==> itemId
            "item_name" ==> itemName
            "price"     ==> price
        ]
        let viewParam = createObj [
            "currency" ==> currency
            "value"    ==> price
            "items"    ==> [| item |]
        ]
        logEvent(analytics, "view_item", viewParam)
    )

let TrackAddToCart (itemId: string) (itemName: string) (quantity: int) (price: decimal) (currency: string) =
    maybeAnalytics
    |> Option.sideEffect (fun analytics ->
        let item = createObj [
            "item_id"   ==> itemId
            "item_name" ==> itemName
            "price"     ==> price
            "quantity"  ==> quantity
        ]
        let cartParam = createObj [
            "currency" ==> currency
            "value"    ==> price * decimal quantity
            "items"    ==> [| item |]
        ]
        logEvent(analytics, "add_to_cart", cartParam)
    )

let TrackBeginCheckout (items: List<Types.FirebaseItem>) (orderTotal: decimal) (currency: string) =
    maybeAnalytics
    |> Option.sideEffect (fun analytics ->
        let itemsInLibraryFormat : obj[] = items |> List.map (fun item -> item.toLibraryObj()) |> Array.ofList
        let beginCheckoutParam = createObj [
            "currency"       ==> currency
            "value"          ==> orderTotal
            "items"          ==> itemsInLibraryFormat
        ]
        logEvent(analytics, "begin_checkout", beginCheckoutParam)
    )

let TrackPurchase (orderId: string) (items: List<Types.FirebaseItem>) (orderTotal: decimal) (shippingCost: decimal) (currency: string) =
    maybeAnalytics
    |> Option.sideEffect (fun analytics ->
        let itemsInLibraryFormat : obj[] = items |> List.map (fun item -> item.toLibraryObj()) |> Array.ofList
        let purchaseParam = createObj [
            "transaction_id" ==> orderId
            "currency"       ==> currency
            "value"          ==> orderTotal
            "shipping"       ==> shippingCost
            "items"          ==> itemsInLibraryFormat
        ]
        logEvent(analytics, "purchase", purchaseParam)
    )

type GoogleAnalyticsTelemetrySink (config: Types.FirebaseConfig) =
    do (initializeApp (createObj [
        "apiKey"        ==> config.ApiKey
        "appId"         ==> config.AppId
        "measurementId" ==> config.MeasurementId
        "projectId"     ==> config.ProjectId
    ])
    |> initialize)

    interface ITelemetrySink with
        override _.TrackEvent (_: string) (_: TelemetryUser) (_: TelemetryProperties) : unit = ()

        override _.TrackScreenView (url: string) (user: TelemetryUser) (_: TelemetryProperties) : unit =
            maybeAnalytics
            |> Option.sideEffect (fun analytics ->
                match user with
                | TelemetryUser.Identified (userId, _) ->
                    setUserId (analytics, userId)
                | TelemetryUser.Anonymous ->
                    setUserId (analytics, null)

                logEvent (analytics, "screen_view", (createObj [
                        "firebase_screen" ==> url
                    ]))
            )