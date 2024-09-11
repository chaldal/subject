module LibLifeCycleCore.Certificates

open System
open System.IO
open System.Reflection
open System.Security.Cryptography.X509Certificates

let starDotDevDotSubjectDotAppTlsCertificate =
    use stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LibLifeCycleCore.Security.STAR_dev_subject_app.pfx")
    if isNull stream then failwith "TLS Certificate embedded resource was not found"
    use ms = new MemoryStream()
    stream.CopyTo(ms)
    new X509Certificate2(ms.ToArray(), "efefefefef")

let filterAndSortCertificates (now: DateTimeOffset) (certificates: seq<X509Certificate2>) : list<X509Certificate2> =
    let nowLocalDateTime = now.ToLocalTime().DateTime
    certificates
    |> Seq.where (fun x509 ->
        x509.Subject = "CN=*.subject.app"
    )
    |> Seq.where (fun x509 ->
        try
            // Attempt to access the Private Key; this might throw if we don't have access
            x509.PrivateKey |> ignore
            true
        with
        | _ -> false
    )
    |> Seq.where (fun x509 -> x509.NotAfter >= nowLocalDateTime)
    |> Seq.where (fun x509 -> x509.NotBefore <= nowLocalDateTime)
    |> Seq.sortByDescending (fun x509 ->
        x509.NotAfter
    )
    |> Seq.toList

let allSubjectDotAppTlsCertificates =
    new Lazy<list<X509Certificate2>>((fun () ->
        // lazy so it's not evaluated when useDevelopmentCertificate = true
        PlatformCertificateStore.fromCollection
            (fun certificates ->
                // FIXME - DateTime here is the time the process starts up. Ideally we should hook on to certificate expiry
                // and gracefully restart server processes as certificates expire so there is no unexpected downtime
                let now = DateTimeOffset.Now

                certificates
                |> Seq.cast<X509Certificate2>
                |> filterAndSortCertificates now)
    ), (* isThreadSafe *) true)

let getOrleansTlsCertificateAndHostName (ecosystemName: string) (useDevelopmentCertificate: bool) : X509Certificate2 * string =
    if useDevelopmentCertificate then
        (starDotDevDotSubjectDotAppTlsCertificate,
            (sprintf "%s.dev.subject.app" (ecosystemName.ToLowerInvariant())))
    else
        (List.head allSubjectDotAppTlsCertificates.Value,
            (sprintf "%s.subject.app" (ecosystemName.ToLowerInvariant())))
