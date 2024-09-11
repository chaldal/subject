#if !FAKE_BUILD_FSPROJ
#load "../../Fake/Includes.fsx"
#load "./compiler/build-compiler.fsx"
#endif

namespace Egg.Shell.Meta.AppRenderDslCompiler

module Build =
    open Egg.Shell.Fake

    type Target =
    | PrepareCompiler
    | Link
    | Clean

    let targetFactory =
        BuildFile.forPath __SOURCE_DIRECTORY__ __SOURCE_FILE__
        |> BuildFile.withTargetsFromUnionType
        |> BuildFile.withDefinition (fun buildContext _normalizePath ->
            function
            | PrepareCompiler ->
                BuildFile.onlyProcessDependencies
            | Link ->
                NodeNpm.publishLink buildContext
            | Clean ->
                NodeNpm.unpublishLink buildContext
            )
        |> BuildFile.withDefaultTarget Link
        |> BuildFile.withInternalDependency (PrepareCompiler >=> Link)
        |> BuildFile.withExternalDependency Compiler.Build.targetFactory
            (Compiler.Build.Target.Publish >=> PrepareCompiler)
        |> BuildFile.initialize