module ThirdParty.ReactNativeDeviceInfo.DeviceInfo

open Fable.Core
open Fable.Core.JsInterop

#if !EGGSHELL_PLATFORM_IS_WEB
let private DeviceInfo: obj -> obj = importDefault "react-native-device-info"

let uniqueId () : Option<NonemptyString> =
    DeviceInfo?getUniqueId() |> NonemptyString.ofString

let getVersion () : string =
    DeviceInfo?getVersion()

let isBatteryCharging () : Async<bool> =
    promise {
        return DeviceInfo?isBatteryCharging()
    } |> Async.AwaitPromise

let getBatteryLevel () : Async<float> =
    promise {
        return DeviceInfo?getBatteryLevel()
    } |> Async.AwaitPromise

let isLocationEnabled () : Async<bool> =
    promise {
        return DeviceInfo?isLocationEnabled()
    } |> Async.AwaitPromise

let getBaseOs (): Async<string> =
    promise {
        return DeviceInfo?getBaseOs()
    } |> Async.AwaitPromise

let getBuildId (): Async<string> =
    promise {
        return DeviceInfo?getBuildId()
    } |> Async.AwaitPromise

let getBrand (): Async<string> =
    promise {
        return DeviceInfo?getBrand()
    } |> Async.AwaitPromise

let getCarrier (): Async<string> =
    promise {
        return DeviceInfo?getCarrier()
    } |> Async.AwaitPromise

let getDevice (): Async<string> =
    promise {
        return DeviceInfo?getDevice()
    } |> Async.AwaitPromise

let getDeviceName (): Async<string> =
    promise {
        return DeviceInfo?getDeviceName()
    } |> Async.AwaitPromise

let getManufacturer (): Async<string> =
    promise {
        return DeviceInfo?getManufacturer()
    } |> Async.AwaitPromise

let getModel (): Async<string> =
    promise {
        return DeviceInfo?getModel()
    } |> Async.AwaitPromise

let getTotalMemory (): Async<uint64> =
    promise {
        return DeviceInfo?getTotalMemory()
    } |> Async.AwaitPromise

let getBuildNumber (): Async<string> =
    promise {
        return DeviceInfo?getBuildNumber()
    } |> Async.AwaitPromise

let getSystemVersion (): Async<string> =
    promise {
        return DeviceInfo?getSystemVersion()
    } |> Async.AwaitPromise

#endif
