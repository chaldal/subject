namespace LibLifeCycleHost.Web

open LibLifeCycle
open Microsoft.AspNetCore.DataProtection
open System.Text

type ApiSessionCryptographer (dataProtectionProvider: IDataProtectionProvider) =
    let purpose = "LibLifeCycleHost: Prevent Session Hijacking"

    let cryptographer: ICryptographer =
        LibLifeCycleHost.HostExtensions.getCryptographer "Egg.Identity" dataProtectionProvider

    member _.Encrypt (decrypted: string) : byte[] =
        let bytes = Encoding.UTF8.GetBytes decrypted
        cryptographer.Encrypt bytes purpose

    member _.Decrypt (encrypted: byte[]) : Result<string, DecryptionFailure> =
        cryptographer.Decrypt encrypted purpose
        |> Result.map Encoding.UTF8.GetString