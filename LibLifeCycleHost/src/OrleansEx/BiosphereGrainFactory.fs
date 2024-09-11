[<AutoOpen>]
module LibLifeCycleHost.BiosphereGrainFactory

open System.Threading
open LibLifeCycle
open LibLifeCycleCore.Certificates
open LibLifeCycleHost
open LibLifeCycleHost.OrleansEx.SiloBuilder
open LibLifeCycleHost.TelemetryModel
open Orleans
open System.Reflection
open System.Threading.Tasks

type BiosphereConnectionException(message:string, innerException: System.Exception) =
   inherit System.Exception(message, innerException)

// Lazy creation of remote grain factories.  Why not just create them eagerly, is it expensive?
// Probably expensive, but more importantly we only know statically referenced ecosystems but not dynamic.

type BiosphereGrainProvider (
        hostEcosystem: Ecosystem,
        hostEcosystemGrainFactory: IGrainFactory,
        membershipConnectionString: string,
        operationTracker: OperationTracker,
        useDevelopmentCertificate: bool,
        buildAssembly: Assembly) =
    let semaphore = new SemaphoreSlim ((* maxCount *) 1)
    let mutable remoteClientsByEcosystemName: Map<string, IClusterClient> = Map.empty
with
    interface IBiosphereGrainProvider with
        member this.GetGrainFactory (ecosystemName: string) =
            if hostEcosystem.Name = ecosystemName then
                // lock-free shortcut for host ecosystem (string comparison penalty should be negligible)
                Task.FromResult hostEcosystemGrainFactory
            else
                match remoteClientsByEcosystemName.TryFind ecosystemName with
                | Some factory ->
                    Task.FromResult factory
                | None ->
                    // interlocked creation. ConcurrentDictionary isn't good enough because GetOrAdd can invoke valueFactory more than once.
                    backgroundTask {

                        do! semaphore.WaitAsync()
                        try
                            // see if was created concurrently
                            match remoteClientsByEcosystemName.TryFind ecosystemName with
                            | Some factory ->
                                return factory :> IGrainFactory
                            | None ->
                                let (orleansTlsCertificate, orleansHostName) =
                                    getOrleansTlsCertificateAndHostName ecosystemName useDevelopmentCertificate
                                let client =
                                    ClientBuilder()
                                        .ConfigureSiloClientForEcosystem(
                                            hostEcosystem,
                                            buildAssembly,
                                            EcosystemSiloClientSetup.HostToRemoteHost
                                                { OrleansTlsCertificate = orleansTlsCertificate
                                                  OrleansHostName = orleansHostName
                                                  MembershipConnectionString = membershipConnectionString
                                                  OperationTracker = operationTracker
                                                  RemoteEcosystemName = ecosystemName })
                                            |> fun clientBuilder -> clientBuilder.Build()

                                try
                                    do!
                                        client
                                            // TODO: auto-close client if unused for a while
                                            .Connect(
                                                fun _exnBeforeRetry ->
                                                    // no need to retry *initial* connection endlessly to avoid blocking loops but
                                                    // must establish connection successfully before caching the client.
                                                    // Later connection may drop & recover automatically.
                                                    Task.FromResult false
                                                )
                                            .ConfigureAwait(false)
                                with
                                | ex ->
                                    BiosphereConnectionException ($"Unable to connect to referenced ecosystem %s{ecosystemName}", ex)
                                    |> fun ex -> TransientSubjectException("GetGrainFactory", ex.ToString())
                                    |> raise

                                remoteClientsByEcosystemName <-
                                    remoteClientsByEcosystemName
                                    |> Map.add
                                        ecosystemName
                                        client
                                return client :> IGrainFactory
                        finally
                            semaphore.Release() |> ignore
                    }

        member _.IsHostedLifeCycle (lcKey: LifeCycleKey) =
            hostEcosystem.Name = lcKey.EcosystemName

        member this.Close () =
            match hostEcosystemGrainFactory with
            | :? IClusterClient as hostEcosystemClient ->
                Some hostEcosystemClient
            | _ -> None
            :: (remoteClientsByEcosystemName.Values |> Seq.map Some |> List.ofSeq)
            |> List.choose id
            |> Array.ofList
            |> Array.map (fun client -> client.Close())
            |> Task.WhenAll