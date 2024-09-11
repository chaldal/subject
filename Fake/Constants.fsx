module Egg.Shell.Fake.Constants

open System.IO

// Build path must be kept in sync with Meta/LibEggshell/src/index.ts
type Target = Web | Native
    with override this.ToString() =
            match this with Web -> "web" | Native -> "native"

type Step = Fable | Bundle | Final
    with override this.ToString() =
            match this with Fable -> "fable" | Bundle -> "bundle" | Final -> "final"

let buildRootPath = ".build"

let buildPathFrom (dir: string) (target: Target) (step: Step) =
    Path.Combine(dir, buildRootPath, string target, string step)

let buildPath (target: Target) (step: Step) =
    buildPathFrom "." target step

let webPackagePath =
    buildPath Web Final

let webPackagePathFrom dir =
    buildPathFrom dir Web Final
