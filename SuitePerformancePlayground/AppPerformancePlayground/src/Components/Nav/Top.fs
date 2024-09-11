[<AutoOpen>]
module AppPerformancePlayground.Components.NavTop

open Fable.React
open LibClient.Components
open LibRouter.Components
open AppPerformancePlayground.Navigation
open AppPerformancePlayground
open LibClient

let private handheld (_nav: Navigation) (maybeCurrentRoute: Option<Route>) () : ReactElement = element {
    LR.Nav.Top.BackButton()
    match maybeCurrentRoute with
    | None -> LC.Nav.Top.Heading("Not Found")
    | Some route ->
        match route with
        | Landing -> LC.Nav.Top.Heading "Landing"
        | Bananas -> LC.Nav.Top.Heading "Bananas"
        | Mangoes -> LC.Nav.Top.Heading "Mangoes"

    LC.Nav.Top.ShowSidebarButton()
}

let private desktopPrefixedHeading (value: string) : ReactElement =
    LC.Nav.Top.Heading $"AppPerformancePlayground » {value}"

let private desktop (_nav: Navigation) (maybeCurrentRoute: Option<Route>) () : ReactElement = element {
    // NOTE abusing the Result type here, what I really want is a choice CE, but don't have one
    let heading = resultful {
        match maybeCurrentRoute with
        | None -> return "Not Found"
        | Some route ->
            match route with
            | Landing -> return "Landing"
            | Bananas -> return "Bananas"
            | Mangoes -> return "Mangoes"
    }

    match heading with
    | Ok value           -> desktopPrefixedHeading value
    | Error reactElement -> reactElement
}


type Ui.Nav with
    [<Component>]
    static member Top (maybeRoute: Option<Route>) =
        LC.Nav.Top.Base (
            handheld = handheld nav maybeRoute,
            desktop  = desktop nav maybeRoute
        )
