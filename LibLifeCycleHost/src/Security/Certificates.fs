module LibLifeCycleHost.Certificates

open System
open System.Security.Cryptography.X509Certificates
open System.Security.Cryptography

[<CLIMutable>]
type CertificateConfiguration = {
    UseDevelopmentCertificate: bool
}

let tryGetPreferredSubjectDotAppTlsCertificate (now:DateTimeOffset) (allTlsCertificates: list<X509Certificate2>) =
    let nowUtc = now.UtcDateTime

    let hasNotExpired (x509: X509Certificate2) = x509.NotAfter >= nowUtc
    let hasValidityStarted (x509: X509Certificate2) = x509.NotBefore <= nowUtc

    allTlsCertificates
    |> Seq.sortByDescending (fun x509 ->
        // Preference in the following order: Expiry Date in the Future, Issue date in the past, Latest Expiry Date
        (hasNotExpired x509), (hasValidityStarted x509), x509.NotAfter
    )
    |> Seq.tryHead

let getSecretsCertificatesFromStore (ecosystemName: string) =
    PlatformCertificateStore.fromCollection
        (fun certificates ->
            certificates
            |> Seq.cast<X509Certificate2>
            |> Seq.where (fun x509 ->
                (ecosystemName.ToLower() |> sprintf "CN=%s.subject.dev.secrets") = x509.Subject)
            |> Seq.toList
        )

let getPreferredSecretsCertificate (now:DateTimeOffset) (allSecretsCertificates: list<X509Certificate2>) =
    let nowUtc = now.UtcDateTime

    allSecretsCertificates
    |> Seq.minBy (fun x509 ->
        (x509.NotAfter <= nowUtc), (x509.NotBefore >= nowUtc), x509.NotAfter
    )


let private isWildcardDomainMatch (wildcardDomain: string) (domain: string) =
    let wildcardDomainWithoutStar = wildcardDomain.[1..]
    let wildcardIndex = domain.IndexOf(wildcardDomainWithoutStar)
    if wildcardIndex = -1 then
        false
    elif wildcardIndex + wildcardDomainWithoutStar.Length = domain.Length then
        domain.Substring(0, wildcardIndex).IndexOf('.') = -1
    else
        false

let private isWildcardDomainName (maybeWildcardDomain: string) =
    maybeWildcardDomain.StartsWith "*." &&
        maybeWildcardDomain.[2..].IndexOf("*") = -1

let private getSubjectDnsName (x509: X509Certificate2) =
    if x509.Subject.StartsWith "CN=" then
        x509.Subject.[3..]
    else
        x509.Subject

let prepareSniCertificateSelector (certificates: list<X509Certificate2>) : (DateTimeOffset -> string -> Option<X509Certificate2>) =
    let sanNamesAndX509 =
        certificates
        |> Seq.collect (fun x509 ->
            x509.Extensions
            |> Seq.cast<X509Extension>
            |> Seq.where(fun extn -> extn.Oid.Value = (*SAN OID*) "2.5.29.17")
            |> Seq.collect (fun extn ->
                // A trick to easily read the SAN name without getting into dirty details
                let asnData = AsnEncodedData(extn.Oid, extn.RawData)
                asnData.Format(true).Split([| "\r\n"; "DNS Name=" |], StringSplitOptions.RemoveEmptyEntries)
            )
            |> Seq.map (fun sanName -> (sanName, x509))
            |> Seq.append (
                ((getSubjectDnsName x509), x509)
                |> Seq.singleton
            )
        )
        |> Seq.map (fun (sanName, x509) -> ((sanName.ToLowerInvariant()), x509))
        |> Seq.groupBy fst
        |> Seq.map (fun (sanName, group) -> (sanName, (group |> Seq.map snd |> Seq.toList)))
        |> Seq.toList

    let (wildcardSanNamesToX509, nonWildcardSanNamesToX509) =
        sanNamesAndX509
        |> Seq.groupBy (fun (sanName, _) -> isWildcardDomainName sanName)
        |> Seq.map (fun (isWildCard, group) -> (isWildCard, dict group))
        |> Map.ofSeq
        |> fun map ->
            let wildcardNamesToX509 =
                map.TryFind true
                |> Option.defaultWith (fun _ -> dict Seq.empty)

            let nonWildcardNamesToX509 =
                map.TryFind false
                |> Option.defaultWith (fun _ -> dict Seq.empty)

            (wildcardNamesToX509, nonWildcardNamesToX509)

    let tryNonWildcardList (now: DateTimeOffset) (ignoreInvalid: bool) sniName =
        let nowUtc = now.UtcDateTime
        if nonWildcardSanNamesToX509.ContainsKey sniName then
            nonWildcardSanNamesToX509.[sniName]
            |> Seq.tryFind (fun x509 -> ignoreInvalid || (x509.NotAfter >= nowUtc && x509.NotBefore <= nowUtc))
        else
            None

    let tryWildcardList (now: DateTimeOffset) (ignoreInvalid: bool) sniName =
        let nowUtc = now.UtcDateTime

        wildcardSanNamesToX509
        |> Seq.map (|KeyValue|)
        |> Seq.where (fun (wildcardDomain, _) -> isWildcardDomainMatch wildcardDomain sniName)
        |> Seq.map snd
        |> Seq.tryHead
        |> Option.bind (
            fun x509s ->
                x509s
                |> Seq.tryFind (fun x509 -> ignoreInvalid || (x509.NotAfter >= nowUtc && x509.NotBefore <= nowUtc))
        )

    fun (now: DateTimeOffset) sniName ->
         tryNonWildcardList now false sniName
         |> Option.orElseWith (fun _ -> tryWildcardList now false sniName)
         |> Option.orElseWith (fun _ -> tryNonWildcardList now true sniName)
         |> Option.orElseWith (fun _ -> tryWildcardList now true sniName)