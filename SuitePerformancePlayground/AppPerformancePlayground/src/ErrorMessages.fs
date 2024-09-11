[<AutoOpen>]
module AppPerformancePlayground.ErrorMessages

open LibUiSubject.Services.SubjectService

type NoOpError with
    member this.ToDisplayString : string = unionCaseName this

// For every SomethingOpError type, provide a ToDisplayString
// function (with a meaningful, human-readable implementation,
// i.e. not unionCaseName), and then provide overrides inside the
// OpErrors (3 per type) and OpErrorsWithPossibleTimeout (1 per type) types

// type ShipmentOpError with
//     member this.ToDisplayString : string = unionCaseName this


type OpErrors =
    static member ToDisplayString (value: NoOpError) : string = value.ToDisplayString
    // static member ToDisplayString (value: ShipmentOpError) : string = value.ToDisplayString

    static member ToDisplayString (value: ActionOrConstructionError<NoOpError>) : string = value.ToDisplayString OpErrors.ToDisplayString
    // static member ToDisplayString (value: ActionOrConstructionError<ShipmentOpError>) : string = value.ToDisplayString OpErrors.ToDisplayString

    static member MapToDisplayString (result: Async<Result<'T, ActionOrConstructionError<NoOpError>>>) : Async<Result<'T, string>> = result |> Async.Map (Result.mapError OpErrors.ToDisplayString)
    // static member MapToDisplayString (result: Async<Result<'T, ActionOrConstructionError<ShipmentOpError>>>) : Async<Result<'T, string>> = result |> Async.Map (Result.mapError OpErrors.ToDisplayString)

type OpErrorsWithPossibleTimeout =
    // static member MapToDisplayString (result: Async<Result<ActOrConstructAndWaitOnLifeEventResult<'T, ShipmentLifeEvent>, ActionOrConstructionError<ShipmentOpError>>>) : Async<Result<unit, string>> =
    //     OpErrorsWithPossibleTimeout.HandleResultIgnoringSuccessResult
    //         result
    //         LifeEventErrors.ToResult
    //         OpErrors.ToDisplayString
    class end
