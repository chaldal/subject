[<AutoOpen>]
module ResultExtensions

open System

#if FABLE_COMPILER
module Result =
    // This is only necessary until we upgrade to the version of Fable that adds the JS native implementation of the method.
    // At such a time, this function can be deleted.
    let toOption (source: Result<'T, 'E>) : Option<'T> =
        match source with
        | Error _  -> None
        | Ok value -> Some value
#else
type Microsoft.FSharp.Core.Result<'T,'Error> with
    static member toOption (source: Result<'T, 'E>) : Option<'T> =
        match source with
        | Error _  -> None
        | Ok value -> Some value
#endif



type Microsoft.FSharp.Core.Result<'T,'Error> with
    static member liftList<'T, 'Error> (listOfResults: List<Result<'T, 'Error>>) : Result<List<'T>, List<'Error>> =
        let (oks, errors) =
            List.foldBack
                (fun curr (oks, errors) ->
                    match curr with
                    | Ok ok       -> (ok :: oks, errors)
                    | Error error -> (oks, error :: errors)
                )
                listOfResults ([], [])

        match errors.IsEmpty with
        | true  -> Ok oks
        | false -> Error errors

    static member liftFirst<'T, 'E> (listOfResults: List<Result<'T, 'E>>) : Result<List<'T>, 'E> =
        Result.liftList listOfResults
        |> Result.mapError List.head

    static member ofOption<'T, 'E> (noneCase: 'E) (option: Option<'T>) : Result<'T, 'E> =
        match option with
        | None       -> Error noneCase
        | Some value -> Ok value

    /// <summary>
    /// Unwraps a <see cref="Result"/>, returning the contained value if it is <c>Ok</c>.
    /// Throws an <see cref="Exception"/> if the result is <c>Error</c>.
    /// </summary>
    /// <param name="result">The candidate</param>
    /// <returns>The contained value if <c>Ok</c>.</returns>
    /// <exception cref="Exception">Thrown if the result is <c>Error</c> with error contents.</exception>
    /// <example>
    /// <code>
    /// let value = Ok 42 |> Result.unwrap  // Returns 42
    /// let errValue = Error "Oops" |> Result.unwrap  // Throws Exception
    /// </code>
    /// </example>
    static member unwrap<'T, 'E> (result: Result<'T, 'E>) : 'T =
        match result with
        | Error e -> failwith $"Called Result.Unwrap on an Error: {e}"
        | Ok value -> value

    /// <summary>
    /// Unwraps a <see cref="Result"/>, returning the contained value if it is <c>Ok</c>.
    /// Throws an <see cref="Exception"/> with the provided message if the result is <c>Error</c>.
    /// </summary>
    /// <param name="expectReason">The error message to include in the exception if the result is <c>Error</c>.</param>
    /// <param name="result">The candidate</param>
    /// <returns>The contained value if <c>Ok</c>.</returns>
    /// <exception cref="Exception">Thrown with the provided message if the result is <c>Error</c>.</exception>
    /// <example>
    /// <code>
    /// let value = Ok "Success" |> Result.expect "Expected Ok value"  // Returns "Success"
    /// let errValue = Error "Oops" |> Result.expect "Expected Ok, but got Error"  // Throws Exception with the given message
    /// </code>
    /// </example>
    static member expect<'T, 'E> (expectReason: string) (result: Result<'T, 'E>) : 'T =
        match result with
        | Error _ -> failwith expectReason
        | Ok value -> value

    static member invert (source: Result<'T, 'E>) : Result<'E, 'T> =
        match source with
        | Error e -> Ok    e
        | Ok    t -> Error t

    static member tryRecover<'T, 'E> (f: 'E -> Result<'T, 'E>) (input: Result<'T, 'E>) : Result<'T, 'E> =
        match input with
        | Error e -> f e
        | _ -> input

    static member recover<'T, 'E> (f: 'E -> 'T) (input: Result<'T, 'E>) : 'T =
        match input with
        | Error e -> f e
        | Ok    t -> t

    static member mapBoth (f: 'T -> 'T2) (g: 'Error -> 'Error2) (input: Result<'T, 'Error>) : Result<'T2, 'Error2> =
        match input with
        | Error e -> g e |> Error
        | Ok t    -> f t |> Ok

    member this.IsError : bool =
        match this with
        | Error _ -> true
        | Ok    _ -> false

    member this.IsOkay : bool =
        not this.IsError

// A Result, where the Ok value is Disposable
type DisposableResult<'T, 'TError when 'T :> IDisposable> = DisposableResult of Result<'T, 'TError>
with
    member this.Result =
        let (DisposableResult res) = this
        res

    interface IDisposable with
        member this.Dispose() =
            match this with
            | DisposableResult(Ok okValue) ->
                okValue.Dispose()
            | _ ->
                ()