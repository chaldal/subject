#if !FAKE_BUILD_FSPROJ
#load "./PackageReferences.fsx"
#load "./BuildFile.fsx"
#endif

namespace Egg.Shell.Fake

module NodeNpm =

    open Fake.Core
    open Fake.JavaScript
    open System
    open System.IO

    type NpmSetParams = Fake.JavaScript.Npm.NpmParams -> Fake.JavaScript.Npm.NpmParams

    let private getHashFilePath (buildContext: BuildFile.BuildContext) hashSubject =
        buildContext.NormalizePath (sprintf "./node_modules/.eggfake.%s.v1.hash" hashSubject)

    let private getCurrentHash (buildContext: BuildFile.BuildContext) hashSubject =
        Directory.CreateDirectory(buildContext.NormalizePath  "./node_modules") |> ignore
        try
            File.ReadAllText((getHashFilePath buildContext hashSubject), Text.Encoding.UTF8)
        with
        | _ -> ""

    let private clearHashFiles (buildContext: BuildFile.BuildContext) (hashSubjects: list<string>) =
        for hashSubject in hashSubjects do
            try
                getHashFilePath buildContext hashSubject
                |> File.Delete
            with
            | _ -> () // NO-OP

    let private npmSetParams (buildContext: BuildFile.BuildContext) : NpmSetParams =
        fun npmParams ->
            { npmParams with WorkingDirectory = buildContext.NormalizePath BuildFile.Current }

    let private setHash (buildContext: BuildFile.BuildContext) hashSubject hash =
        File.WriteAllText((getHashFilePath buildContext hashSubject), hash, Text.Encoding.UTF8)

    let private getPackageLockHash (buildContext: BuildFile.BuildContext) =
        let md5 = CryptHash.Net.Hash.Hash.MD5()
        let packageJsonFullPath = buildContext.NormalizePath "./package.json"
        let packageLockFullPath = buildContext.NormalizePath "./package-lock.json"
        let hash1 =
            if File.Exists packageJsonFullPath then
                let bytes = File.ReadAllBytes packageJsonFullPath
                (md5.ComputeHash bytes).HashBytes
            else
                Array.empty

        let hash2 =
            if File.Exists packageLockFullPath then
                let bytes = File.ReadAllBytes packageLockFullPath
                (md5.ComputeHash bytes).HashBytes
            else
                Array.empty

        Array.append hash1 hash2
        |> Convert.ToBase64String

    let publishLink (buildContext: BuildFile.BuildContext) =
        let hash = getPackageLockHash buildContext
        let currentHashFileContents = getCurrentHash buildContext "link"
        if hash <> currentHashFileContents then
            Npm.exec "link --prefer-offline --audit=false --loglevel=error" (npmSetParams buildContext)
            setHash buildContext "link" hash

    let unpublishLink (buildContext: BuildFile.BuildContext) =
        Npm.exec "unlink --no-save --prefer-offline --audit=false --loglevel=error" (npmSetParams buildContext)

    let link (projects: string list) (buildContext: BuildFile.BuildContext) =
        let md5 = CryptHash.Net.Hash.Hash.MD5()
        let currentHashFileContents = getCurrentHash buildContext "link-projects"
        let projNames = String.concat " " projects
        let projectJsonHash = getPackageLockHash buildContext
        let hash =
            projNames
            |> sprintf "%s %s" projectJsonHash
            |> Text.Encoding.UTF8.GetBytes
            |> fun bytes -> (md5.ComputeHash bytes).HashBytes
            |> Convert.ToBase64String

        if hash <> currentHashFileContents then
            Npm.exec (sprintf "link %s --prefer-offline --audit=false --no-save --loglevel=error" projNames) (npmSetParams buildContext)
            setHash buildContext "link-projects" hash

    let willInstall (buildContext: BuildFile.BuildContext) =
        let hash = getPackageLockHash buildContext
        let currentHashFileContents = getCurrentHash buildContext "install"
        currentHashFileContents <> hash

    let forceInstall (buildContext: BuildFile.BuildContext) : unit =
        let hash = getPackageLockHash buildContext
        Npm.exec "install --prefer-offline --audit=false --loglevel=error" (npmSetParams buildContext)
        // First clear out all hashes, as all operations likely need to be recomputed
        clearHashFiles buildContext ["install"; "link-projects"; "link"]
        setHash buildContext "install" hash

    let install (buildContext: BuildFile.BuildContext) =
        // TODO - remove once all pipeline caches updated
        // Had to run without --prefer-offline param, because in build pipleline
        // It was failing to lookup for some required node modules.
        // Once `--prefer-offline` param is removed it works fine!
        Npm.exec "install --audit=false --loglevel=error" (npmSetParams buildContext)
        // let hash = getPackageLockHash buildContext
        // let currentHashFileContents = getCurrentHash buildContext "install"
        // if currentHashFileContents <> hash then
        //     forceInstall buildContext

    let unlink (projects: string list) (buildContext: BuildFile.BuildContext) =
        let projNames = String.concat " " projects
        Npm.exec (sprintf "unlink %s --prefer-offline --audit=false --no-save --loglevel=error" projNames) (npmSetParams buildContext)

    let run (cmd: string) (buildContext: BuildFile.BuildContext) =
        Npm.exec (sprintf "run %s --prefer-offline --audit=false --loglevel=error" cmd) (npmSetParams buildContext)

    let publish (buildContext: BuildFile.BuildContext) =
        Npm.exec "publish" (npmSetParams buildContext)

    let private createPlatformExecutable
            (sourceDirectory: string)
            (outputPath:      string)
            (targetPlatform:  string) =
        let nexePath =
            Path.Combine(__SOURCE_DIRECTORY__, "./node_modules/nexe/index.js")
            |> Path.GetFullPath

        CreateProcess.fromRawCommand "node" [
            nexePath
            "./server.js"
            $"--target={targetPlatform}"
            "--resource=./**/*"
            // nexe does not glob dotfiles (it uses globby internally, which has 'dot' defaulting to false)
            // so we need to include this explicitly.
            "--resource=public/.well-known"
            (sprintf "--output=%s" outputPath)
        ]
        |> CreateProcess.withWorkingDirectory sourceDirectory
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore

    let createWindowsExecutable (sourceDirectory: string) (outputPath: string) =
        createPlatformExecutable sourceDirectory outputPath "windows-x64-12.18.2"

    [<System.Obsolete("Use createWindowsExecutable")>]
    let createExecutable (sourceDirectory: string) (outputPath: string) =
        createWindowsExecutable sourceDirectory outputPath

    let createLinuxExecutable (sourceDirectory: string) (outputPath: string) =
        createPlatformExecutable sourceDirectory outputPath "linux-x64-12.16.2"