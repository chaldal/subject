module LibService.Config

[<RequireQualifiedAccess>]
type LoadConfigError =
| FilenameNotProvided of string
| IOError             of System.Exception
| DecodingError       of string
| VerificationError   of string

let decodeConfig<'T> (source: string) : Result<'T, LoadConfigError> =
    Thoth.Json.Net.Decode.Auto.fromString<'T> source
    |> Result.mapError LoadConfigError.DecodingError

let private verifyConfig<'T> (config: 'T) (verifier: 'T -> Option<string>) : Result<'T, LoadConfigError> =
    match verifier config with
    | None       -> Ok config
    | Some error -> Error (LoadConfigError.VerificationError error)

let decodeAndVerifyConfig<'T> (source: string) (verifier: 'T -> Option<string>) : Result<'T, LoadConfigError> = resultful {
    let! config = decodeConfig source
    return! verifyConfig config verifier
}

let loadConfig<'T> (path: string) : Result<'T, LoadConfigError> =
    try
        let source = System.IO.File.ReadAllText path
        decodeConfig source
    with
    | error -> Error (LoadConfigError.IOError error)

let loadAndVerifyConfig<'T> (path: string) (verifier: 'T -> Option<string>) : Result<'T, LoadConfigError> = resultful {
    let! config = loadConfig<'T> path
    return! verifyConfig config verifier
}

let maybeApplyEnvironmentVariableOverride<'T> (configValue: 'T) (environmentVariableName: string) (decoder: string -> 'T) : 'T =
    match System.Environment.GetEnvironmentVariable environmentVariableName |> Option.ofObj with
    | None                          -> configValue
    | Some environmentVariableValue -> decoder environmentVariableValue
