#if !FAKE_BUILD_FSPROJ
#load "./PackageReferences.fsx"
#load "./Operators.fsx"
#load "./BuildFile.fsx"
#endif

namespace Egg.Shell.Fake

module EggShellCli =
    open Fake.Core
    open Fake.IO

    // type EggshellCommand =
    // | CreateApp
    // | CreateComponent
    // | RenameComponent
    // | CreateRoute
    // | CreateDialog
    // | CreateThirdPartyWrapper
    // | TestBuild
    // | BuildLib
    // | DevNative
    // | DevWeb
    // | DevAndroid
    // | DevNativeServer
    // | PackageAndroid
    // | PackageWeb

    let eggShellJsLocation =
        Path.combine __SOURCE_DIRECTORY__ "../Meta/eggshell.js"
        |> System.IO.Path.GetFullPath

    // let private eggshellArgumentFor eggshellCommand =
    //     match eggshellCommand with
    //     | CreateApp -> "create-app"
    //     | CreateComponent -> "create-component"
    //     | RenameComponent -> "rename-component"
    //     | CreateRoute -> "create-route"
    //     | CreateDialog -> "create-dialog"
    //     | CreateThirdPartyWrapper -> "create-third-party-wrapper"
    //     | TestBuild -> "test-build"
    //     | BuildLib -> "build-lib"
    //     | DevNative -> "dev-native"
    //     | DevWeb -> "dev-web"
    //     | DevAndroid -> "dev-android"
    //     | DevNativeServer -> "dev-native-server"
    //     | PackageAndroid -> "package-android"
    //     | PackageWeb -> "package-web"

    let private compilerRelativePath =
        sprintf "node_modules/react-templates-fable/compiler/bin/Release/net7.0/%s/AppRenderDslCompiler%s"
            currentDotNetRuntimeIdentifier (if currentDotNetRuntimeIdentifier = "win-x64" then ".exe" else "")

    let private executeEggshell workingDirectory command (maybeAdditionalParams: list<string>) =
        let args =
            [
                eggShellJsLocation
                command
                (sprintf "--compiler=%s" compilerRelativePath)
            ] @ maybeAdditionalParams

        CreateProcess.fromRawCommand "node" args
        |> CreateProcess.ensureExitCode
        |> CreateProcess.withWorkingDirectory workingDirectory
        |> CreateProcess.setEnvironmentVariable "NODE_OPTIONS" "--max-old-space-size=4096"
        |> Proc.run
        |> ignore

    let execute (buildContext: BuildFile.BuildContext) (command: string) =
        executeEggshell (buildContext.NormalizePath ".") command [ ]

    // TODO: remove? replace with type-safe version for internal calls?
    let buildLib (buildContext: BuildFile.BuildContext) =
        executeEggshell (buildContext.NormalizePath ".") "build-lib" []

    // TODO: remove? replace with type-safe version for internal calls?
    let packageWeb (buildContext: BuildFile.BuildContext) =
        executeEggshell (buildContext.NormalizePath ".") "package-web" []
