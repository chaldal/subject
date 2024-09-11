#if !FAKE_BUILD_FSPROJ
#load "./PackageReferences.fsx"
#load "./BuildFile.fsx"
#endif

namespace Egg.Shell.Fake

module DotNetEx =

    open Fake.DotNet
    open Egg.Shell.Fake

    // DotNet restores that run concurrently sometimes interferes with each other
    // So we'll protect them with a mutex

    let private lockObj = obj()

    let restore setParams project =
        lock lockObj (fun _ ->
            DotNet.restore setParams project
        )

    // Build may do an implicit restore
    let build (setParams: DotNet.BuildOptions -> DotNet.BuildOptions) project =
        DotNet.build (fun options ->
            let options = setParams options
            if not options.NoRestore then
                // Do a locked restore first
                lock lockObj (fun _ ->
                    DotNet.restore (fun restoreOptions ->
                        { restoreOptions with
                            Common =
                                { restoreOptions.Common with
                                    Verbosity = Some DotNet.Verbosity.Quiet }
                            Runtime = Some currentDotNetRuntimeIdentifier
                            }) project
                )
                { options with NoRestore = true }
            else
                options
        ) project

    // Publish may do an implicit restore
    let publish (setParams: DotNet.PublishOptions -> DotNet.PublishOptions) project =
        DotNet.publish (fun options ->
            let options = setParams options
            if not options.NoRestore then
                // Do a locked restore first
                lock lockObj (fun _ ->
                    DotNet.restore (fun restoreOptions ->
                        { restoreOptions with
                            Common =
                                { restoreOptions.Common with
                                    Verbosity = Some DotNet.Verbosity.Quiet }
                            Runtime = Some currentDotNetRuntimeIdentifier
                            }) project
                )
                { options with NoRestore = true }
            else
                options
        ) project