module ThirdParty.ReactNativeContacts.Contacts

open Fable.Core
open Fable.Core.JsInterop
open FSharp.Data
open LibClient

#if !EGGSHELL_PLATFORM_IS_WEB
let private PermissionsAndroid: obj        = import "PermissionsAndroid"  "react-native";
let private Contacts:           obj -> obj = importDefault "react-native-contacts"

type PhoneNumber = 
    {
        id:     string
        label:  string
        number: string
    }

type Contact = 
    {
        name:  string
        phone: List<string>
    }
    static member fromJsObj(jsContact: obj) : Contact =
        let givenName  = jsContact?givenName 
        let familyName = jsContact?familyName 
        let middleName = jsContact?middleName

        let phoneNumebrs = (jsContact?phoneNumbers :> obj :?> PhoneNumber[])

        {
            name  = $"{givenName} {middleName} {familyName}"
            phone = phoneNumebrs |> Array.map (fun num -> num.number) |> Array.distinct |> Array.toList
        }

let getAllContactsForAndroid () : Async<List<Contact>> =
    async {
        let! granted        = PermissionsAndroid?request("android.permission.READ_CONTACTS") |> Async.AwaitPromise
        let grantedAsString = granted :> obj :?> string

        if grantedAsString = "granted" then
            let! allContacts = Contacts?getAllWithoutPhotos() |> Async.AwaitPromise
            return allContacts |> Array.map(fun c -> Contact.fromJsObj c) |> Array.toList
        else
            Fable.Core.JS.console.log("Contacts Permission Denied")
            return List.empty
    }

let getContactsPermissionForIos () : Async<bool> =
    async {
        let! granted        = Contacts?checkPermission() |> Async.AwaitPromise
        let grantedAsString = granted :> obj :?> string

        if grantedAsString = "authorized" then
            return true
        else
            let! permission        = Contacts?requestPermission() |> Async.AwaitPromise
            let permissionAsString = permission :> obj :?> string

            return permissionAsString = "authorized"
    }

let getAllContactsForIos () : Async<List<Contact>> =
    async {
        let! granted        = Contacts?checkPermission() |> Async.AwaitPromise
        let grantedAsString = granted :> obj :?> string

        if grantedAsString = "authorized" then
            let! allContacts = Contacts?getAllWithoutPhotos() |> Async.AwaitPromise
            return
                allContacts |> Array.map(fun c -> Contact.fromJsObj c) |> Array.toList
        else
            Fable.Core.JS.console.log("Contacts Permission Denied")
            return List.empty
    }
#endif