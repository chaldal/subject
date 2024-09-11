module ThirdParty.GoogleAnalytics.Base

open LibClient

let Initialize (config: Types.FirebaseConfig)=
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.GoogleAnalyticsTelemetrySink config
    |> addTelemetrySink
    #else
    config |> ignore
    Native.GoogleAnalyticsTelemetrySink ()
    |> addTelemetrySink
    #endif

let TrackViewItem (itemId: string) (itemName: string) (price: decimal) (currency: string) =
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.TrackViewItem itemId itemName price currency
    #else
    Native.TrackViewItem itemId itemName price currency
    #endif

let TrackAddToCart (itemId: string) (itemName: string) (quantity: int) (price: decimal) (currency: string) =
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.TrackAddToCart itemId itemName quantity price currency
    #else
    Native.TrackAddToCart itemId itemName quantity price currency
    #endif

let TrackPurchase (orderId: string) (items: List<Types.FirebaseItem>) (orderTotal: decimal) (shippingCost: decimal) (currency: string) =
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.TrackPurchase orderId items orderTotal shippingCost currency
    #else
    Native.TrackPurchase orderId items orderTotal shippingCost currency
    #endif

let TrackBeginCheckout (items: List<Types.FirebaseItem>) (orderTotal: decimal) (currency: string) : unit =
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.TrackBeginCheckout items orderTotal currency
    #else
    Native.TrackBeginCheckout items orderTotal currency
    #endif