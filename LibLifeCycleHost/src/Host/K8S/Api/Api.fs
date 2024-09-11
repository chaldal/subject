module LibLifeCycleHost.Host.K8S.Api

open LibLifeCycle
open LibLifeCycleHost.Host
open LibLifeCycleHost.Host.K8S
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System
open System.Reflection
open System.Threading.Tasks

// TODO: Untangle and separate Dev and Prod configurations. It's spaghetti at the moment, can't tell one from another
// TODO End goal: same Api setup code for all environments, no duplication

let startApiWebHost
        (hostConfiguration: HostConfiguration)
        (ecosystem':        Ecosystem)
        (buildAssembly:     Assembly)
        (args:              array<string>)
        : int =
    try
        WebHost.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                fun _hostContext configurationBuilder ->
                    // even when reloadOnChange = false, default file provider still creates a file system watcher, which can exhaust system limits in linux
                    let fileProviderThatDoesNotWatchFileSystem =
                        let fileProvider = configurationBuilder.GetFileProvider()
                        { new Microsoft.Extensions.FileProviders.IFileProvider with
                            member _.GetFileInfo (subpath: string) = fileProvider.GetFileInfo subpath
                            member this.GetDirectoryContents (subpath: string) = fileProvider.GetDirectoryContents subpath
                            member this.Watch (_filter: string) = Microsoft.Extensions.FileProviders.NullChangeToken.Singleton }
                    configurationBuilder
                        .SetFileProvider(fileProviderThatDoesNotWatchFileSystem)
                        .AddJsonFile(
                            "appsettings.json",
                            optional       = false,
                            reloadOnChange = false
                        )
                        .AddCommandLine(args)
                        .AddEnvironmentVariables()
                    |> ignore
            )
            .ConfigureServices(
                fun context services ->
                    let ecosystem = DevEnv.applyDevEnvConfigToEcosystem ecosystem' context.Configuration
                    services
                        .AddSingleton<Ecosystem>(ecosystem)
                        .AddSingleton<BuildAssembly>({Assembly = buildAssembly})
                        .AddSingleton<HostConfiguration>(hostConfiguration)
                    |> ignore
            )
            .ConfigureKestrel(
                fun context options ->
                    context.Configuration.GetSection "Kestrel"
                    |> options.Configure
                    |> ignore
            )
            .UseStartup<Startup>()
            .UseKestrel(
                fun kestrelServerOptions ->
                    kestrelServerOptions.AddServerHeader              <- false
                    kestrelServerOptions.Limits.MaxResponseBufferSize <- int64 (256 * 1024) // 256 KB
                    kestrelServerOptions.Limits.MaxRequestBodySize    <- int64 LibLifeCycleHost.Web.Http.MaxDeflatedBodySize
            )
            .Build()
            .RunAsync()
            .Wait()
        with
        | :? AggregateException as exn when exn.InnerExceptions.Count = 1 && exn.InnerExceptions.[0].GetType() = typeof<TaskCanceledException> ->
            printfn "Shutting Down .."
            ()

        | :? TaskCanceledException ->
            printfn "Shutting Down .."
            ()
    0
