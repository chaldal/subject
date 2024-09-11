#if !FAKE_BUILD_FSPROJ
#load "./PackageReferences.fsx"
#load "./BuildFile.fsx"
#endif

namespace Egg.Shell.Fake

module TypeScript =

    open Fake.Core

    let invokeCompiler (normalizePath: BuildFile.CurrentDirPathNormalizer) =
        let cmd =
            if Environment.isWindows then
                normalizePath "./node_modules/.bin/tsc.cmd"
            else
                normalizePath "./node_modules/.bin/tsc"

        CreateProcess.fromRawCommandLine cmd "--build src/tsconfig.json"
        |> CreateProcess.withWorkingDirectory (normalizePath ".")
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore