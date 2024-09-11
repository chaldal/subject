module Scraping.RenderDslCodeSnippets

open System.IO

open Thoth.Json.Net
open Scraping.Types


let private readFileContent (FilePath filePath) : Result<string, SnippetError> =
    try
        Ok (File.ReadAllText filePath)
    with error ->
        error |> sprintf "%A" |> SnippetError.FileRead |> Error

type private SnippetValue = {
  Prefix:      string
  Description: string
  Body:        List<string>
  Scope:       string
}

let private snippetValueDecoder : Decoder<SnippetValue> =
    Decode.object (fun fields -> {
      Prefix      = fields.Required.At ["prefix"]      Decode.string
      Description = fields.Required.At ["description"] Decode.string
      Body        = fields.Required.At ["body"]        (Decode.list Decode.string)
      Scope       = fields.Required.At ["scope"]       Decode.string
    })

let getRenderDslSnippetDataResultEncoded () : string =
    resultful {
        // Path hardcoded since this is a service script
        let! fileContentResult = readFileContent (FilePath "../../Meta/renderdsl.code-snippets")
        let! decodedSnippetData =
            Decode.fromString (Decode.keyValuePairs snippetValueDecoder) fileContentResult
            |> Result.mapError SnippetError.Decode
            |> Result.map (List.map (fun (key, snippetValue) ->
                {
                    Key         = key
                    Prefix      = snippetValue.Prefix
                    Description = snippetValue.Description
                    Body        = snippetValue.Body
                    Scope       = snippetValue.Scope
                }
            ))
        return decodedSnippetData
    }
    |> fun result -> Encode.Auto.toString (4, result)
