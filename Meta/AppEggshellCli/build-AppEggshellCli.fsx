#if !FAKE_BUILD_FSPROJ
#load "../../Fake/Includes.fsx"
#load "../LibScaffolding/build-LibScaffolding.fsx"
#load "../../LibLangTypeScript/build-LibLangTypeScript.fsx"
#load "../LibEggshell/build-LibEggshell.fsx"
#load "../LibRtCompilerFileSystemBindings/build-LibRtCompilerFileSystemBindings.fsx"
#endif

namespace Egg.Shell.Meta.AppEggshellCli

module Build =
    open Fake.IO
    open Egg.Shell.Fake

    type Target =
    | InitializeDependencies
    | Build
    | Clean

    let targetFactory =
        BuildFile.forPath __SOURCE_DIRECTORY__ __SOURCE_FILE__
        |> BuildFile.withTargetsFromUnionType
        |> BuildFile.withDefinition (fun buildContext normalizePath ->
            function
            | InitializeDependencies ->
                NodeNpm.install buildContext
            | Build ->
                TypeScript.invokeCompiler normalizePath

            | Clean ->
                Shell.rm_rf (normalizePath "./node_modules")
                Shell.rm_rf (normalizePath "./dist")
            )
        |> BuildFile.withDefaultTarget Build
        |> BuildFile.withInternalDependency (InitializeDependencies >=> Build)
        |> BuildFile.withExternalDependency Egg.Shell.Meta.LibScaffolding.Build.targetFactory
            (Egg.Shell.Meta.LibScaffolding.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.LibLangTypeScript.Build.targetFactory
            (Egg.Shell.LibLangTypeScript.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.Meta.LibEggshell.Build.targetFactory
            (Egg.Shell.Meta.LibEggshell.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.Meta.LibRtCompilerFileSystemBindings.Build.targetFactory
            (Egg.Shell.Meta.LibRtCompilerFileSystemBindings.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.Meta.LibScaffolding.Build.targetFactory
            (Egg.Shell.Meta.LibScaffolding.Build.Target.Build >=> Build)
        |> BuildFile.withExternalDependency Egg.Shell.LibLangTypeScript.Build.targetFactory
            (Egg.Shell.LibLangTypeScript.Build.Target.Build >=> Build)
        |> BuildFile.withExternalDependency Egg.Shell.Meta.LibEggshell.Build.targetFactory
            (Egg.Shell.Meta.LibEggshell.Build.Target.Build >=> Build)
        |> BuildFile.withExternalDependency Egg.Shell.Meta.LibRtCompilerFileSystemBindings.Build.targetFactory
            (Egg.Shell.Meta.LibRtCompilerFileSystemBindings.Build.Target.Build >=> Build)
        |> BuildFile.initialize