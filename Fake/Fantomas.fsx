#if !FAKE_BUILD_FSPROJ
#load "./PackageReferences.fsx"
#endif

namespace Egg.Shell.Fake

module Fantomas =

    open Fake.Core
    open Fake.IO

    let private executeFantomas workingDirectory (maybeAdditionalParams: list<string>) =
        let args = "fantomas" :: maybeAdditionalParams

        CreateProcess.fromRawCommand "dotnet" args
        |> CreateProcess.ensureExitCode
        |> CreateProcess.withWorkingDirectory workingDirectory
        |> Proc.run

    let checkFormatting workingDirectory: unit =
        executeFantomas workingDirectory [ "--check"; "--recurse"; "." ]
        |> ignore
