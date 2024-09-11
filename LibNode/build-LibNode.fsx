#if !FAKE_BUILD_FSPROJ
#load "../Fake/Includes.fsx"
#load "../LibLangTypeScript/build-LibLangTypeScript.fsx"
#endif

namespace Egg.Shell.LibNode

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
                NodeNpm.publishLink buildContext

            | Build ->
                TypeScript.invokeCompiler normalizePath

            | Clean ->
                NodeNpm.unpublishLink buildContext
                Shell.rm_rf (normalizePath "./node_modules")
            )
        |> BuildFile.withDefaultTarget InitializeDependencies
        |> BuildFile.withInternalDependency (InitializeDependencies >=> Build)
        |> BuildFile.withExternalDependency Egg.Shell.LibLangTypeScript.Build.targetFactory
            (Egg.Shell.LibLangTypeScript.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.LibLangTypeScript.Build.targetFactory
            (Egg.Shell.LibLangTypeScript.Build.Target.Build >=> Build)
        |> BuildFile.initialize