[<AutoOpen>]
module LibClient.Restart

open LibClient
open LibClient.Services.ImageService
open LibClient.Components
open ReactXP.Components
open ReactXP.Helpers
open LibClient.JsInterop

let private restartElement =
    LC.Centered (
        RX.ActivityIndicator (
            color = "#cccccc"
        )
    )

let restartApp (element: ReactElement) () : unit =
    ReactXPRaw?UserInterface?setMainView restartElement

    runLater (TimeSpan.FromMilliseconds 2) (fun () ->
        ReactXPRaw?UserInterface?setMainView element
    )
