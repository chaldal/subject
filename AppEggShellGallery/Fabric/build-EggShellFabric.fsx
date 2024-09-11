#if !FAKE_BUILD_FSPROJ
#load "../../Fake/Includes.fsx"
#load "../../Fake/build-Fake.fsx"
#load "../build-AppEggShellGallery.fsx"
#endif

namespace Egg.Shell.AppGallery.Fabric

module Build =

    open Fake.IO
    open Egg.Shell.Fake

    type Target =
    | PackageGalleryForWindowsDeployment
    | Clean

    let targetFactory =
        BuildFile.forPath __SOURCE_DIRECTORY__ __SOURCE_FILE__
        |> BuildFile.withTargetsFromUnionType
        |> BuildFile.withDefinition (fun _buildContext normalizePath ->
            function
            | PackageGalleryForWindowsDeployment ->
                NodeNpm.createExecutable (normalizePath (Constants.webPackagePathFrom "..")) (normalizePath "./ApplicationPackageRoot/Egg.Shell.GalleryPkg/Code/server.exe")

            | Clean ->
                Shell.rm (normalizePath "./ApplicationPackageRoot/Egg.Shell.GalleryPkg/Code/server.exe")
            )
        |> BuildFile.withDefaultTarget PackageGalleryForWindowsDeployment
        |> BuildFile.withExternalDependency Egg.Shell.AppEggShellGallery.Build.targetFactory
            (Egg.Shell.AppEggShellGallery.Build.Target.InstallPackageDependencies >=> PackageGalleryForWindowsDeployment)
        |> BuildFile.withExternalDependency Egg.Shell.Fake.Build.targetFactory
            (Egg.Shell.Fake.Build.Target.InitializeDependencies >=> PackageGalleryForWindowsDeployment)
        |> BuildFile.initialize