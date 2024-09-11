#if !FAKE_BUILD_FSPROJ
#load "./PackageReferences.fsx"
#endif

namespace Egg.Shell.Fake

[<AutoOpen>]
module Operators =

    open Fake.Core

    type Dependency<'T, 'U> = {
        Op: 'T
        DependentOp: 'U
        IsHardDependency: bool
    }

    let inline (>=>) (op: 'T) (dependentOp: 'U) =
        { Op = op; DependentOp = dependentOp; IsHardDependency = true }

    let inline (>~>) (op: 'T) (dependentOp: 'U) =
        { Op = op; DependentOp = dependentOp; IsHardDependency = false }

    let currentDotNetRuntimeIdentifier =
        Environment.environVarOrNone "Egg_RuntimeIdenifier"
        |> Option.defaultValue (
            if Environment.isWindows then
                "win-x64"
            elif Environment.isMacOS then
                "osx-x64"
            elif Environment.isLinux then
                "linux-x64"
            else
                failwith "Unsupported Operating System"
        )