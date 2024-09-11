module ReactXP.NetInfo

open Fable.Core
open Fable.Core.JS
open LibLangFsharp
open Fable.Core.JsInterop

let private netInfoRaw: obj = importDefault "@chaldal/reactxp-netinfo"

let isConnected () : Async<bool> =
    let deferred = Deferred<bool>()
    netInfoRaw?isConnected()?``then``(deferred.Resolve)
    deferred.Value

let onIsConnectedChange (callback: bool -> unit) : unit =
    netInfoRaw?connectivityChangedEvent?subscribe(callback)

#if !EGGSHELL_PLATFORM_IS_WEB
type WifiDetails =
    abstract ipAddress:             string
    abstract isConnectionExpensive: bool
    abstract ssid:                  string
    abstract strength:              Option<int>
    abstract subnet:                string

type CellularDetails =
    abstract carrier:               string
    abstract cellularGeneration:    LibClient.JsInterop.JsNullable<string>
    abstract isConnectionExpensive: bool
    
type ReactNativeNetInfo =
    abstract details:             Option<obj>
    abstract isConnected:         bool
    abstract isInternetReachable: bool
    abstract ``type``:            string
    
type ReactNativeNetInfoLibrary =
    abstract fetch: unit -> Promise<ReactNativeNetInfo>
    
let private RNNetInfo: ReactNativeNetInfoLibrary = importDefault "@react-native-community/netinfo";

type NetDetails =
| Cellular of CellularDetails
| Wifi of WifiDetails
| Others of string
| Unknown
| NoConnection

type NetInfo = {
    Details:             NetDetails
    IsConnected:         bool
    IsInternetReachable: bool
}

let getNetInfoState () : Async<NetInfo>  =
    promise {
        let! netInfoRaw =  RNNetInfo.fetch()
        let details =
            match netInfoRaw.``type``, netInfoRaw.details with
            | "cellular", Some details ->
                Cellular (box details :?> CellularDetails)
            | "wifi", Some details ->
                Wifi (box details :?> WifiDetails)
            | "none", _    -> NoConnection
            | "unknown", _ -> Unknown
            | _, _         -> Others (netInfoRaw.``type``)
        
        let netInfo = {
            Details             = details
            IsConnected         = netInfoRaw.isConnected
            IsInternetReachable = netInfoRaw.isInternetReachable
        }

        return netInfo
    } |> Async.AwaitPromise
#endif
