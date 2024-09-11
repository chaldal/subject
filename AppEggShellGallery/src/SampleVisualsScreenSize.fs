module AppEggShellGallery.SampleVisualsScreenSize

open Fable.React
open LibClient
open LibClient.Responsive

let sampleVisualsScreenSizeContext = ReactBindings.React.createContext ScreenSize.Desktop

let sampleVisualsScreenSizeContextProvider: ScreenSize -> ReactElements -> ReactElement = contextProvider sampleVisualsScreenSizeContext
