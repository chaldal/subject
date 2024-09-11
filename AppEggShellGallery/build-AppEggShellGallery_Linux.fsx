#if !FAKE_BUILD_FSPROJ
#load "../Fake/Includes.fsx"
#load "../Fake/build-Fake.fsx"
#load "./build-AppEggShellGallery.fsx"
#endif

namespace Egg.Shell.AppGallery.Linux

module Build =

    open Fake.IO
    open Egg.Shell.Fake

    type Target =
    | PackageGalleryForLinuxDeployment
    | CleanLinux
    
    let targetFactory =
        BuildFile.forPath __SOURCE_DIRECTORY__ __SOURCE_FILE__
        |> BuildFile.withTargetsFromUnionType
        |> BuildFile.withDefinition (fun _buildContext normalizePath ->
            function
            | PackageGalleryForLinuxDeployment ->
                NodeNpm.createLinuxExecutable (normalizePath (Constants.webPackagePathFrom ".")) (normalizePath "./app/server")

            | CleanLinux ->
                Shell.rm (normalizePath "./app/server")
            )
        |> BuildFile.withDefaultTarget PackageGalleryForLinuxDeployment
        |> BuildFile.withExternalDependency Egg.Shell.AppEggShellGallery.Build.targetFactory
            (Egg.Shell.AppEggShellGallery.Build.Target.InstallPackageDependencies >=> PackageGalleryForLinuxDeployment)
        |> BuildFile.withExternalDependency Egg.Shell.Fake.Build.targetFactory
            (Egg.Shell.Fake.Build.Target.InitializeDependencies >=> PackageGalleryForLinuxDeployment)
        |> BuildFile.initialize