#if !FAKE_BUILD_FSPROJ
#load "../Fake/Includes.fsx"
#endif

namespace Egg.Shell.LibLangTypeScript

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
                Shell.rm_rf (normalizePath "./dist")
            )
        |> BuildFile.withDefaultTarget Build
        |> BuildFile.withInternalDependency (InitializeDependencies >=> Build)
        |> BuildFile.initialize