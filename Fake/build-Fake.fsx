#if !FAKE_BUILD_FSPROJ
#load "./Includes.fsx"
#endif

namespace Egg.Shell.Fake

module Build =
    open Fake.IO
    open Egg.Shell.Fake

    type Target =
    | InitializeDependencies
    | Clean

    let targetFactory =
        BuildFile.forPath __SOURCE_DIRECTORY__ __SOURCE_FILE__
        |> BuildFile.withTargetsFromUnionType
        |> BuildFile.withDefinition (fun buildContext normalizePath ->
            function
            | InitializeDependencies ->
                NodeNpm.install buildContext

            | Clean ->
                NodeNpm.unpublishLink buildContext
                Shell.rm_rf (normalizePath "./node_modules")
            )
        |> BuildFile.withDefaultTarget InitializeDependencies
        |> BuildFile.initialize