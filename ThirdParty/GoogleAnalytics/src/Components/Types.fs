module ThirdParty.GoogleAnalytics.Types

open Fable.Core.JsInterop


type FirebaseConfig = {
    ApiKey:        string
    AppId:         string
    MeasurementId: string
    ProjectId:     string
}

type FirebaseItem =
    {
        ItemId: string
        Price:  decimal
    }
    member this.toLibraryObj () : obj =
        createObj [
            "item_id" ==> this.ItemId
            "price"   ==> float this.Price
        ]

