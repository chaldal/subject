#if !FAKE_BUILD_FSPROJ
#load "../Fake/Includes.fsx"
#load "../build-AllLibraries.fsx"
#load "../Meta/AppRenderDslCompiler/build-AppRenderDslCompiler.fsx"
#load "../Meta/AppEggshellCli/build-AppEggshellCli.fsx"
#load "ImagePicker/build-ImagePicker.fsx"
#load "Map/build-Map.fsx"
#load "QRCode/build-QRCode.fsx"
#load "ReactNativeCodePush/build-ReactNativeCodePush.fsx"
#load "ReactNativeContacts/build-ReactNativeContacts.fsx"
#load "ReactNativeDeviceInfo/build-ReactNativeDeviceInfo.fsx"
#load "Recharts/build-Recharts.fsx"
#load "Showdown/build-Showdown.fsx"
#load "SyntaxHighlighter/build-SyntaxHighlighter.fsx"
#load "FacebookPixel/build-FacebookPixel.fsx"
#load "GoogleAnalytics/build-GoogleAnalytics.fsx"
#load "ReCaptcha/build-ReCaptcha.fsx"
#load "SunmiPrint/build-SunmiPrint.fsx"
#endif

namespace Egg.Shell.ThirdParty

module Build =
    open Fake.IO
    open Fake.DotNet
    open Fake.Core
    open Egg.Shell.Fake

    type Target =
    | InitializeDependencies
    | Clean
    | EggShellBuildLib
    | CreateNewThirdPartyLibrary

    let targetFactory =
        BuildFile.forPath __SOURCE_DIRECTORY__ __SOURCE_FILE__
        |> BuildFile.withTargetsFromUnionType
        |> BuildFile.withDefinition (fun buildContext _normalizeDir ->
            function
            | InitializeDependencies -> ()

            | Clean -> ()

            | EggShellBuildLib -> ()

            | CreateNewThirdPartyLibrary ->
                EggShellCli.execute buildContext "create-third-party-wrapper"
            )
        |> BuildFile.withDefaultTarget InitializeDependencies

        // InitializeDependencies
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ImagePicker.Build.targetFactory
            (Egg.Shell.ThirdParty.ImagePicker.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.Map.Build.targetFactory
            (Egg.Shell.ThirdParty.Map.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReactNativeCodepush.Build.targetFactory
            (Egg.Shell.ThirdParty.ReactNativeCodepush.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReactNativeContacts.Build.targetFactory
            (Egg.Shell.ThirdParty.ReactNativeContacts.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReactNativeDeviceInfo.Build.targetFactory
            (Egg.Shell.ThirdParty.ReactNativeDeviceInfo.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.Recharts.Build.targetFactory
            (Egg.Shell.ThirdParty.Recharts.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.Showdown.Build.targetFactory
            (Egg.Shell.ThirdParty.Showdown.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.SyntaxHighlighter.Build.targetFactory
            (Egg.Shell.ThirdParty.SyntaxHighlighter.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.FacebookPixel.Build.targetFactory
            (Egg.Shell.ThirdParty.FacebookPixel.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.GoogleAnalytics.Build.targetFactory
            (Egg.Shell.ThirdParty.GoogleAnalytics.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReCaptcha.Build.targetFactory
            (Egg.Shell.ThirdParty.ReCaptcha.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.SunmiPrint.Build.targetFactory
            (Egg.Shell.ThirdParty.SunmiPrint.Build.Target.InitializeDependencies >=> InitializeDependencies)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.QRCode.Build.targetFactory
            (Egg.Shell.ThirdParty.QRCode.Build.Target.InitializeDependencies >=> InitializeDependencies)

        // Clean
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ImagePicker.Build.targetFactory
            (Egg.Shell.ThirdParty.ImagePicker.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.Map.Build.targetFactory
            (Egg.Shell.ThirdParty.Map.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReactNativeCodepush.Build.targetFactory
            (Egg.Shell.ThirdParty.ReactNativeCodepush.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReactNativeContacts.Build.targetFactory
            (Egg.Shell.ThirdParty.ReactNativeContacts.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReactNativeDeviceInfo.Build.targetFactory
            (Egg.Shell.ThirdParty.ReactNativeDeviceInfo.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.Recharts.Build.targetFactory
            (Egg.Shell.ThirdParty.Recharts.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.Showdown.Build.targetFactory
            (Egg.Shell.ThirdParty.Showdown.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.SyntaxHighlighter.Build.targetFactory
            (Egg.Shell.ThirdParty.SyntaxHighlighter.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.FacebookPixel.Build.targetFactory
            (Egg.Shell.ThirdParty.FacebookPixel.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.GoogleAnalytics.Build.targetFactory
            (Egg.Shell.ThirdParty.GoogleAnalytics.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReCaptcha.Build.targetFactory
            (Egg.Shell.ThirdParty.ReCaptcha.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.SunmiPrint.Build.targetFactory
            (Egg.Shell.ThirdParty.SunmiPrint.Build.Target.Clean >=> Clean)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.QRCode.Build.targetFactory
            (Egg.Shell.ThirdParty.QRCode.Build.Target.Clean >=> Clean)

        // EggShellBuildLib
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ImagePicker.Build.targetFactory
            (Egg.Shell.ThirdParty.ImagePicker.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.Map.Build.targetFactory
            (Egg.Shell.ThirdParty.Map.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReactNativeCodepush.Build.targetFactory
            (Egg.Shell.ThirdParty.ReactNativeCodepush.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReactNativeContacts.Build.targetFactory
            (Egg.Shell.ThirdParty.ReactNativeContacts.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReactNativeDeviceInfo.Build.targetFactory
            (Egg.Shell.ThirdParty.ReactNativeDeviceInfo.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.Recharts.Build.targetFactory
            (Egg.Shell.ThirdParty.Recharts.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.Showdown.Build.targetFactory
            (Egg.Shell.ThirdParty.Showdown.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.SyntaxHighlighter.Build.targetFactory
            (Egg.Shell.ThirdParty.SyntaxHighlighter.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.FacebookPixel.Build.targetFactory
            (Egg.Shell.ThirdParty.FacebookPixel.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.GoogleAnalytics.Build.targetFactory
            (Egg.Shell.ThirdParty.GoogleAnalytics.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.ReCaptcha.Build.targetFactory
            (Egg.Shell.ThirdParty.ReCaptcha.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.SunmiPrint.Build.targetFactory
            (Egg.Shell.ThirdParty.SunmiPrint.Build.Target.EggShellBuildLib >=> EggShellBuildLib)
        |> BuildFile.withExternalDependency Egg.Shell.ThirdParty.QRCode.Build.targetFactory
            (Egg.Shell.ThirdParty.QRCode.Build.Target.EggShellBuildLib >=> EggShellBuildLib)

        |> BuildFile.initialize