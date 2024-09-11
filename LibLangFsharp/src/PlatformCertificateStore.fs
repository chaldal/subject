module PlatformCertificateStore

#if !FABLE_COMPILER

open System.IO
open System.Runtime.InteropServices
open System.Security.Cryptography.X509Certificates

[<Literal>]
let private wellKnownNonWindowsMountedCertDirectory = "/etc/certs"

let nonWindowsLazyCertStore =
    Lazy<X509Certificate2Collection>((fun () ->
        let allCerts =
            // search subdirectories recursively, expect *.crt files paired with *.key files
            Directory.GetFiles(wellKnownNonWindowsMountedCertDirectory, "*.crt", SearchOption.AllDirectories)
            |> Seq.map (fun certFile -> certFile, Path.ChangeExtension (certFile, ".key"))
            |> Seq.map (fun (certFile, keyFile) -> X509Certificate2.CreateFromPemFile (certFile, keyFile))
            |> List.ofSeq
        // deduplicate! k8s can have the same certs repeated as symlinks, or it can be just config such as same cert under different secret names
        allCerts
        |> List.groupBy id // F# uses custom equality here, not reference equality
        |> List.map (fun (_, group) -> List.head group)
        |> Array.ofList
        |> X509Certificate2Collection
        ),
        (* isThreadSafe *) true)

let isWindows = RuntimeInformation.IsOSPlatform OSPlatform.Windows

let fromCollection (f: X509Certificate2Collection -> 'T) : 'T =
    if isWindows then
        use store = new X509Store(StoreName.My, StoreLocation.LocalMachine)
        store.Open(OpenFlags.ReadOnly)
        f store.Certificates
    else
        f nonWindowsLazyCertStore.Value

#endif
