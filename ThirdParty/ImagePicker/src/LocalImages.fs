module ThirdParty.ImagePicker.LocalImages

open LibClient.Services.ImageService

let mutable private localImageImplementation: string -> ImageSource =
    fun (_filename: string) -> failwith "Please provide localImage implementation to ThirdParty.ImagePicker"


let localImage (filename: string) : ImageSource =
    localImageImplementation filename

let provideImplementation (implementation: string -> ImageSource) : unit =
    localImageImplementation <- implementation