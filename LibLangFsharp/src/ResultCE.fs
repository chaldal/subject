[<AutoOpen>]
module ResultCE

type ResultBuilder() =
    member _.Bind(x: Result<'T1, 'E>, f: 'T1 -> Result<'T2, 'E>) : Result<'T2, 'E> =
        match x with
        | Error error -> Error error
        | Ok value -> f value

    member _.Return(value: 'T): Result<'T, 'E> =
        Ok value

    member _.ReturnFrom(wrappedValue: 'T) : 'T =
        wrappedValue

    member this.Zero () : Result<unit, 'Error> =
        this.Return ()

let resultful = ResultBuilder()

// Note: this may be overkill for AutoOpen...
let (|>>) (a: Result<'A, 'E>) (b: 'A -> Result<'B, 'E>): Result<'B, 'E> = a |> Result.bind b
