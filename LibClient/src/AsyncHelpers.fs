namespace LibClient

[<AutoOpen>]
module AsyncHelpers =
    // Safely means "logging when errors happen"
    // TODO the stack traces are likely to be somewhat unhelpful, so it would
    // be nice to eventually do something about that.
    let startSafely (what: Async<unit>) : unit =
        async {
            match! what |> Async.TryCatch with
            | Ok _    -> Noop
            | Error e -> Log.Error ("Error in AsyncHelpers.startSafely: {error}", e.ToString())
        }
        |> Async.StartImmediate

