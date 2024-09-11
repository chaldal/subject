#if !FAKE_BUILD_FSPROJ
#load "../../../Fake/Includes.fsx"
#endif

namespace Egg.Shell.Meta.AppRenderDslCompiler.Compiler

module Build =

    open Fake.DotNet
    open Egg.Shell.Fake

    type Target =
    | Clean
    | Build
    | Rebuild
    | Publish

    let targetFactory =
        BuildFile.forPath __SOURCE_DIRECTORY__ __SOURCE_FILE__
        |> BuildFile.withTargetsFromUnionType
        |> BuildFile.withDefinition (fun _ normalizePath ->
            function
            | Clean ->
                Fake.IO.Shell.cleanDirs [(normalizePath "./bin"); (normalizePath "./obj")]

            | Build ->
                DotNetEx.build (fun p ->
                    { p with
                          Configuration = DotNet.Release
                          NoLogo = true
                          Common =
                              { p.Common with
                                    Verbosity = Some DotNet.Verbosity.Quiet } }) (normalizePath ".")

            | Rebuild ->
                // NO-OP
                BuildFile.onlyProcessDependencies

            | Publish ->
                DotNetEx.publish (fun publishOptions ->
                    { publishOptions with
                        Configuration = DotNet.Release
                        NoLogo = true
                        Common =
                            { publishOptions.Common with
                                Verbosity = Some DotNet.Verbosity.Quiet }
                        SelfContained = Some true
                        Runtime = Some currentDotNetRuntimeIdentifier
                     }) (normalizePath ".")
            )
        |> BuildFile.withDefaultTarget Publish
        |> BuildFile.withInternalDependency (Clean >~> Build)
        |> BuildFile.withInternalDependency (Clean >=> Rebuild)
        |> BuildFile.withInternalDependency (Build >=> Rebuild)
        |> BuildFile.initialize