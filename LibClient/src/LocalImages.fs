module LibClient.LocalImages

open LibClient.Services.ImageService

let mutable private localImageImplementation: string -> ImageSource =
    fun (_filename: string) -> failwith "Please provide localImage implementation to LibClient"


let localImage (filename: string) : ImageSource =
    localImageImplementation filename

let provideImplementation (implementation: string -> ImageSource) : unit =
    localImageImplementation <- implementation
