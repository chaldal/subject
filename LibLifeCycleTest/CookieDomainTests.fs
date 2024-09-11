module ``Cookie Domain Tests``

open Xunit
open LibLifeCycleHost.Web.Config

[<Fact>]
let ``No mappings given``() =
    match { DefaultAppCookieDomain = ".foo"; HostNameSuffixToAppCookieDomainQueryString = "" }
        .GetAppCookieDomainForHostName "www.bar" with
    | ".foo" -> ``👍``
    | _ -> ``💣``

[<Fact>]
let ``Mappings are null``() =
    match { DefaultAppCookieDomain = ".foo"; HostNameSuffixToAppCookieDomainQueryString = null }
        .GetAppCookieDomainForHostName "www.bar" with
    | ".foo" -> ``👍``
    | _ -> ``💣``

[<Fact>]
let ``Full hostname is mapped``() =
    match { DefaultAppCookieDomain = ".foo"; HostNameSuffixToAppCookieDomainQueryString = "www.bar.baz=.bar.baz" }
        .GetAppCookieDomainForHostName "www.bar.baz" with
    | ".bar.baz" -> ``👍``
    | _ -> ``💣``

[<Fact>]
let ``Parent hostname is mapped``() =
    match { DefaultAppCookieDomain = ".foo"; HostNameSuffixToAppCookieDomainQueryString = "bar.baz=.bar.baz" }
        .GetAppCookieDomainForHostName "www.bar.baz" with
    | ".bar.baz" -> ``👍``
    | _ -> ``💣``

[<Fact>]
let ``hostname is not mapped``() =
    match { DefaultAppCookieDomain = ".foo"; HostNameSuffixToAppCookieDomainQueryString = ".bar=.bar.baz" }
        .GetAppCookieDomainForHostName "www.bar.baz" with
    | ".foo" -> ``👍``
    | _ -> ``💣``

[<Fact>]
let ``Multiple mappings for the same hostname are provided``() =
    match { DefaultAppCookieDomain = ".foo"; HostNameSuffixToAppCookieDomainQueryString = "www.bar.baz=.bar.baz&www.bar.baz=.another.baz" }
        .GetAppCookieDomainForHostName "www.bar.baz" with
    | ".bar.baz" -> ``👍``
    | _ -> ``💣``

[<Fact>]
let ``Multiple hostnames are mapped``() =
    match { DefaultAppCookieDomain = ".foo"; HostNameSuffixToAppCookieDomainQueryString = "www.bar.baz=.bar.baz&www.another.baz=.another.baz" }
        .GetAppCookieDomainForHostName "www.bar.baz" with
    | ".bar.baz" -> ``👍``
    | _ -> ``💣``