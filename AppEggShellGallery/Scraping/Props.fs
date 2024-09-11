module Scraping.Props

open System.IO
open System.Text.RegularExpressions

open Thoth.Json.Net

open LibRenderDSL.Types
open Scraping.Types


let private getComponentFilesFromDirectory (directoryPath: string) : Result<seq<FilePath>, ScrapeError> =
    try
        Directory.EnumerateFiles(directoryPath, "*.typext.fs", SearchOption.AllDirectories)
        |> Seq.map FilePath
        |> Ok
    with error ->
        error |> sprintf "%A" |> DirectoryError |> Error

let private readFileContent (FilePath filePath) : Result<FilePath * string, ScrapeError> =
    try
        Ok (FilePath filePath, File.ReadAllText filePath)
    with error ->
        error |> sprintf "%A" |> FileReadError |> Error


let private getFullyQualifiedName (FilePath filePath, fileContent: string) : Result<string, ScrapeError> =
    let theMatch = Regex("^module (.*)", RegexOptions.Multiline).Match fileContent

    match theMatch.Success with
    | true  -> Ok (theMatch.Groups.Item(1).Value)
    | false -> filePath |> FullyQualifiedNameNotFound |> Error

let private isProps (record: TaggedRecordType) : bool =
    Regex.IsMatch (record.Name, "^Props(<.*>)?$")

let private getPropsFromFileContent (fileContent: string) : Result<TaggedRecordType, PropsError> =
    LibRenderDSL.RecordsWithDefaults.extractTaggedRecordTypes fileContent
    |> Result.mapError ExtractError
    |> Result.bind (fun candidates ->
        match candidates |> List.filter isProps with
        | [props] -> Ok props
        | []      -> Error NoProps
        | _       -> Error MultipleProps
    )


let private getScrapeResult (directoryPaths: seq<string>) : ScrapeResult =
    let getScrapeResultFromDirectory (directoryPath: string) : ScrapeResult =
        getComponentFilesFromDirectory directoryPath
        |> Result.map (fun componentFilePaths ->
            componentFilePaths
            |> Seq.map readFileContent
            |> Seq.fold
                (fun (acc: ScrapeResult) (currResult: Result<FilePath * string, ScrapeError>) ->
                    resultful {
                        let! (filePath, content) = currResult
                        let! fullyQualifiedName  = getFullyQualifiedName (filePath, content)
                        return { acc with Results = acc.Results.AddOrUpdate (fullyQualifiedName, getPropsFromFileContent content) }
                    }
                    |> Result.recover acc.AddError
                )
                ScrapeResult.Empty
        )
        |> Result.recover ScrapeResult.Empty.AddError

    directoryPaths
    |> Seq.map getScrapeResultFromDirectory
    |> Seq.fold
        (fun (acc: ScrapeResult) (currResult: ScrapeResult) ->
            resultful {
                return {
                    Results = Map.merge acc.Results currResult.Results
                    Errors = acc.Errors @ currResult.Errors
                }
            }
            |> Result.recover acc.AddError
        ) ScrapeResult.Empty

let getPropsDataResult (directoryPaths: List<string>) : string =
    Encode.Auto.toString (4, getScrapeResult directoryPaths)