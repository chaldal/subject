[<AutoOpen>]
module internal LibLifeCycleTest.DataSeeding.SimulationDiscovery

open System.Reflection
open System.Text.RegularExpressions
open System.Threading.Tasks
open FSharp.Reflection
open LibLifeCycleTest

type DiscoveryFilter = {
    Assembly:                     Assembly
    Module:                       Option<Regex>
    Simulation:                   Option<Regex>
    IncludeNonSeedingSimulations: bool
}

type Simulation = {
    ModuleName:     string
    SimulationName: string
    Invoke:         (unit -> obj)
}

let private hasSimulationAttribute (method: MethodInfo) (includeNonSeedingSimulations: bool) =
    let att = method.GetCustomAttribute<SimulationAttribute>()
    match box att with
    | null -> false
    | _ -> includeNonSeedingSimulations || att.IsForDataSeedingOnly

let private hasZeroParameterCount (method: MethodInfo) =
    method.GetParameters()
    |> Seq.isEmpty

let private doesReturnSimulationCExpr (method: MethodInfo) =
    let retType = method.ReturnType

    if FSharpType.IsFunction retType then
        let methodInfo = retType.GetMethod("Invoke")

        if methodInfo <> null then
            let funcParams = methodInfo.GetParameters()
            funcParams.Length = 1 && funcParams.[0].ParameterType = typeof<TestPartition> && typeof<Task>.IsAssignableFrom methodInfo.ReturnType
        else
            false
    else
        false

let discoverSimulations (filter: DiscoveryFilter): List<Simulation> =
    let modules =
        filter.Assembly.GetTypes()
        |> Array.map (fun t -> {| Module = t |})
        |> Array.where (fun info -> FSharpType.IsModule info.Module)
        |> Array.choose
            (
                fun info ->
                    match filter.Module with
                    | None -> Some info
                    | Some regex ->
                        match regex.IsMatch(info.Module.FullName) with
                        | true -> Some info
                        | false -> None
            )
    let methods =
        modules
        |> Array.collect (
            fun info ->
                info.Module.GetMethods(BindingFlags.Public ||| BindingFlags.Static)
                |> Array.map (fun m -> {| Module = info.Module; Method = m |})
            )
        |> Array.where (fun info -> hasZeroParameterCount info.Method)
        |> Array.where (fun info -> hasSimulationAttribute info.Method filter.IncludeNonSeedingSimulations)
        |> Array.where (fun info -> doesReturnSimulationCExpr info.Method)
        |> Array.choose
            (
                fun info ->
                    match filter.Simulation with
                    | None -> Some info
                    | Some regex ->
                        match regex.IsMatch(info.Method.Name) with
                        | true -> Some info
                        | false -> None
            )
    let simulations =
        methods
        |> Array.map
            (
                fun info ->
                    {
                        ModuleName     = info.Module.Name
                        SimulationName = info.Method.Name
                        Invoke         = fun () -> info.Method.Invoke(null, [||])
                    }
            )
        |> List.ofArray

    simulations
