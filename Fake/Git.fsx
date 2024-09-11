#if !FAKE_BUILD_FSPROJ
#load "./PackageReferences.fsx"
#endif

namespace Egg.Shell.Fake

module Git =

    open Fake.Core
    open Fake.IO

    let private executeGit workingDirectory command (maybeAdditionalParams: list<string>) =
        let args = command :: maybeAdditionalParams

        CreateProcess.fromRawCommand "git" args
        |> CreateProcess.ensureExitCode
        |> CreateProcess.withWorkingDirectory workingDirectory
        |> CreateProcess.redirectOutput
        |> Proc.run

    let getHeadSha workingDirectory: string =
        let result = executeGit workingDirectory "rev-parse" [ "--verify"; "HEAD" ]
        result.Result.Output.Trim()