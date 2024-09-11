module ThirdParty.FacebookPixel.Base

open LibClient

let Initialize (facebookPixelId: string)=
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.FacebookPixelTelemetrySink facebookPixelId
    |> addTelemetrySink
    #else
    facebookPixelId |> ignore
    ()
    #endif

let TrackCompleteRegistration () : unit =
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.CompleteRegistration
    #else
    Native.CompleteRegistration ()
    #endif

let TrackInitiateCheckout (contentIds: List<string>) (price: UnsignedDecimal) : unit =
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.InitiateCheckout contentIds price
    #else
    Native.InitiateCheckout contentIds price
    #endif

let TrackViewContent (contentId: string) (price: UnsignedDecimal) : unit =
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.ViewContent contentId price
    #else
    Native.ViewContent contentId price
    #endif

let TrackPurchase (contentsWithIdAndQuantity: List<string * int>) (price: UnsignedDecimal) : unit =
    #if EGGSHELL_PLATFORM_IS_WEB
    let contentIds = contentsWithIdAndQuantity |> List.map (fun (id, _) -> id)
    Web.Purchase contentIds price
    #else
    Native.Purchase contentsWithIdAndQuantity price
    #endif

let TrackAddToCart (contentWithIdAndQuantity: string * int) (price: UnsignedDecimal) : unit =
    #if EGGSHELL_PLATFORM_IS_WEB
    Web.AddToCart contentWithIdAndQuantity price
    #else
    Native.AddToCart contentWithIdAndQuantity price
    #endif