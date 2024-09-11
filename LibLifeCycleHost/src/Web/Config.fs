module LibLifeCycleHost.Web.Config

open LibLifeCycle.Config
open Microsoft.AspNetCore.WebUtilities
open Microsoft.Extensions.Primitives

[<CLIMutable>]
type HttpCookieConfiguration = {
    DefaultAppCookieDomain: string
    HostNameSuffixToAppCookieDomainQueryString: string
}
with
    member this.GetAppCookieDomainForHostName (hostName: string) =
        QueryHelpers.ParseQuery this.HostNameSuffixToAppCookieDomainQueryString
        |> Seq.choose (fun kv ->
            if hostName.EndsWith kv.Key then
                kv.Value.ToArray()
                |> Seq.where(System.String.IsNullOrWhiteSpace >> not)
                |> Seq.tryHead
            else
                None)
        |> Seq.tryHead
        |> Option.defaultValue this.DefaultAppCookieDomain

    interface IValidatable with
        member this.Validate(): unit =
            ()

[<AllowNullLiteral>]
type HttpHandlerSettings (isDevHost: bool) =
    member this.IsDevHost = isDevHost

[<CLIMutable>]
type ApplicationConfiguration = {
    CorsHostNamesCsv: string
}
with
    interface IValidatable with
        member __.Validate () : unit = Noop
