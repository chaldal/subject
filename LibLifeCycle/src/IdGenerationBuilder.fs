namespace LibLifeCycle

open System.Threading.Tasks

[<AutoOpen>]
module IdGenerationBuilder =

    type IdGenerationBuilder() =
        member _.Bind(IdGenerationResult res: IdGenerationResult<'SubjectId1, 'OpError>, binder: 'SubjectId1 -> IdGenerationResult<'SubjectId2, 'OpError>) : IdGenerationResult<'SubjectId2, 'OpError> =
            backgroundTask {
                match! res with
                | Ok subjId ->
                    let (IdGenerationResult binderTask) = binder subjId
                    match! binderTask with
                    | Ok subjId2 ->
                        return Ok subjId2
                    | Error err ->
                        return Error err
                | Error err ->
                    return Error err
            }
            |> IdGenerationResult

        member _.Return(value: 'SubjectId when 'SubjectId :> SubjectId and 'SubjectId : comparison): IdGenerationResult<'SubjectId, 'OpError> =
            Ok value
            |> Task.FromResult
            |> IdGenerationResult

        member _.Return<'SubjectId, 'OpError
                         when 'OpError :> OpError> (error: 'OpError): IdGenerationResult<'SubjectId, 'OpError> =
            Error error
            |> Task.FromResult
            |> IdGenerationResult

        [<CompilerMessage("QQQ Not implemented yet", 10666, IsError=false)>]
        member _.Return(_todo: TODO): IdGenerationResult<'SubjectId, 'OpError> =
            raise (System.NotImplementedException "QQQ Not implemented yet")

        member _.ReturnFrom (res: IdGenerationResult<'SubjectId, 'OpError>) : IdGenerationResult<'SubjectId, 'OpError> =
            res

        member _.ReturnFrom(result: Result<'SubjectId, 'OpError>): IdGenerationResult<'SubjectId, 'OpError> =
            result
            |> Task.FromResult
            |> IdGenerationResult

    let idgen = IdGenerationBuilder()

[<AutoOpen>]
module IdGenerationBuilderExtensions =

    type IdGenerationBuilder with

        member _.ReturnFrom (x: Task<'SubjectId>) : IdGenerationResult<'SubjectId, 'OpError> =
            backgroundTask {
                let! resolved = x
                return Ok resolved
            }
            |> IdGenerationResult

        member _.ReturnFrom (x: Async<'SubjectId>) : IdGenerationResult<'SubjectId, 'OpError> =
            backgroundTask {
                let! resolved = x |> Async.StartAsTask
                return Ok resolved
            }
            |> IdGenerationResult

        member _.Return(value: 'SubjectId): IdGenerationResult<'SubjectId, 'OpError> =
            Ok value
            |> Task.FromResult
            |> IdGenerationResult

        member _.Bind(asyncTask: Task<'SubjectId1>, binder: 'SubjectId1 -> IdGenerationResult<'SubjectId2, 'OpError>) : IdGenerationResult<'SubjectId2, 'OpError> =
            backgroundTask {
                let! resolved = asyncTask
                let (IdGenerationResult binderTask) = binder resolved
                return! binderTask
            }
            |> IdGenerationResult