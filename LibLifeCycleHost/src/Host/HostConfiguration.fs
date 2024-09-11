namespace LibLifeCycleHost.Host

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

type HostConfiguration = {
    ConfigureServices: IConfiguration * IServiceCollection -> unit
    Configure:         IApplicationBuilder -> unit
}