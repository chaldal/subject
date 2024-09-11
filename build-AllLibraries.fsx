#if !FAKE_BUILD_FSPROJ
#load "./Fake/Includes.fsx"
#load "./LibClient/build-LibClient.fsx"
#load "./LibRouter/build-LibRouter.fsx"
#load "./LibUiAdmin/build-LibUiAdmin.fsx"
#load "./LibUiSubject/build-LibUiSubject.fsx"
#load "./LibUiSubjectAdmin/build-LibUiSubjectAdmin.fsx"
// FIXME avoid pulling all 3rd party packages here, as this will unnecessarily increase the build time
// Instead each leaf-node project should just pull in what it needs
#load "./ThirdParty/SyntaxHighlighter/build-SyntaxHighlighter.fsx"
#load "./ThirdParty/Showdown/build-Showdown.fsx"
#load "./ThirdParty/Map/build-Map.fsx"
#load "./ThirdParty/FacebookPixel/build-FacebookPixel.fsx"
#load "./ThirdParty/Recharts/build-Recharts.fsx"
#load "./ThirdParty/ReCaptcha/build-ReCaptcha.fsx"
#load "./LibPushNotification/Client/build-PushNotificationClient.fsx"
#endif

namespace Egg.Shell

module Build =

    open Egg.Shell.Fake

    type Target =
    | PackageAllLibraries
    | InitializeDependenciesForAllLibraries
    | CleanAll

    let targetFactory =
        BuildFile.forPath __SOURCE_DIRECTORY__ __SOURCE_FILE__
        |> BuildFile.withTargetsFromUnionType
        |> BuildFile.withDefinition (fun _ _ ->
            function
            | InitializeDependenciesForAllLibraries -> BuildFile.onlyProcessDependencies
            | PackageAllLibraries -> BuildFile.onlyProcessDependencies
            | CleanAll ->
                // TODO
                ()
            )
        |> BuildFile.withExternalDependency LibClient.Build.targetFactory
            (LibClient.Build.EggShellBuildLib >=> PackageAllLibraries)
        |> BuildFile.withExternalDependency LibRouter.Build.targetFactory
            (LibRouter.Build.EggShellBuildLib >=> PackageAllLibraries)
        |> BuildFile.withExternalDependency LibUiAdmin.Build.targetFactory
            (LibUiAdmin.Build.EggShellBuildLib >=> PackageAllLibraries)
        |> BuildFile.withExternalDependency LibUiSubject.Build.targetFactory
            (LibUiSubject.Build.EggShellBuildLib >=> PackageAllLibraries)
        |> BuildFile.withExternalDependency LibUiSubjectAdmin.Build.targetFactory
            (LibUiSubjectAdmin.Build.EggShellBuildLib >=> PackageAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.SyntaxHighlighter.Build.targetFactory
            (ThirdParty.SyntaxHighlighter.Build.EggShellBuildLib >=> PackageAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.Showdown.Build.targetFactory
            (ThirdParty.Showdown.Build.EggShellBuildLib >=> PackageAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.Map.Build.targetFactory
            (ThirdParty.Map.Build.EggShellBuildLib >=> PackageAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.Recharts.Build.targetFactory
            (ThirdParty.Recharts.Build.EggShellBuildLib >=> PackageAllLibraries)
        |> BuildFile.withExternalDependency LibClient.Build.targetFactory
            (LibClient.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency LibRouter.Build.targetFactory
            (LibRouter.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency LibUiAdmin.Build.targetFactory
            (LibUiAdmin.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency LibUiSubject.Build.targetFactory
            (LibUiSubject.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency LibUiSubjectAdmin.Build.targetFactory
            (LibUiSubjectAdmin.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.SyntaxHighlighter.Build.targetFactory
            (ThirdParty.SyntaxHighlighter.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.Showdown.Build.targetFactory
            (ThirdParty.Showdown.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.Map.Build.targetFactory
            (ThirdParty.Map.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.Recharts.Build.targetFactory
            (ThirdParty.Recharts.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.FacebookPixel.Build.targetFactory
            (ThirdParty.FacebookPixel.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency PushNotificationClient.Build.targetFactory
            (PushNotificationClient.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.withExternalDependency ThirdParty.ReCaptcha.Build.targetFactory
            (ThirdParty.ReCaptcha.Build.InitializeDependencies >=> InitializeDependenciesForAllLibraries)
        |> BuildFile.initialize