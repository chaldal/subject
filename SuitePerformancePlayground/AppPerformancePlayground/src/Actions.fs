module AppPerformancePlayground.Actions

open LibClient
open AppPerformancePlayground.AppServices
open AppPerformancePlayground.ErrorMessages

let logOut () : UDActionResult =
    services().Session.Act (services().Session.CurrentSessionId) SampleSessionAction.Logout
    |> OpErrors.MapToDisplayString

// Actions (often subject actions) that are used from multiple locations can be groupd here