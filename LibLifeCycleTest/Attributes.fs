[<AutoOpen>]
module Attributes

open System
open Xunit
open Xunit.Sdk

[<AttributeUsage(AttributeTargets.Method ||| AttributeTargets.Property, AllowMultiple = false)>]
[<XunitTestCaseDiscoverer("LibLifeCycleTest.TestRunner.SimulationDiscoverer", "LibLifeCycleTest")>]
type public SimulationAttribute() =
    inherit FactAttribute()

    // Setting this to true will make the test runner not discover this test
    // NOTE: If you update the name of this property, you must also update it in TestRunner.fs/SimulationDiscoverer
    member val IsForDataSeedingOnly = false with get, set