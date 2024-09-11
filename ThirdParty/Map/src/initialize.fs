module ThirdParty.Map.Initialize

open LibClient.Services.ImageService

let initialize (localImageImplementation: string -> ImageSource) : unit =
    ThirdParty.Map.LocalImages.provideImplementation localImageImplementation