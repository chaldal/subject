module ThirdParty.ImagePicker.initialize

open LibClient.Services.ImageService

let initialize (localImageImplementation: string -> ImageSource) : unit =
    ThirdParty.ImagePicker.LocalImages.provideImplementation localImageImplementation