module AppEggShellGallery.Actions

open LibClient

let greet (_e: ReactEvent.Action) : unit =
    Browser.Dom.window.alert "Hello!"

let demoPointerEventAction (_e: ReactEvent.Action) : unit =
    Browser.Dom.window.alert "Hello!"