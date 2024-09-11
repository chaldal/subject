module LibClient.Initialize

open LibClient
open LibClient.Services.ImageService
open LibClient.Components

let initialize (localImageImplementation: string -> ImageSource) : unit =
    LibClient.Base64.initialize ()
    LibClient.Polyfill.AbortController.initialize ()
    LibClient.Polyfill.GlobalSelf.initialize ()
    LibClient.LocalImages.provideImplementation localImageImplementation

    LibClient.LegacyTextWrapping.wrapRawText <- (fun (text: string) ->
        LC.Text(children = [|Fable.React.Helpers.str text|])
    )
