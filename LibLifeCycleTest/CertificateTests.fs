module ``Certificate Tests``

open Xunit
open FsUnit.Xunit
open System
open System.Security.Cryptography
open System.Security.Cryptography.X509Certificates
open LibLifeCycleCore.Certificates

let createCertificate (issueDate: DateTimeOffset) (expiryDate: DateTimeOffset) : X509Certificate2 =
    let rsa = RSA.Create()
    let req = new CertificateRequest("CN=*.subject.app", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
    req.CreateSelfSigned(issueDate, expiryDate)

[<Fact>]
let ``Expiried Certificates should be filtered``() =
    let now = new DateTimeOffset(2023, 10, 5, 1, 2, 3, TimeSpan.Zero)

    let certificate = createCertificate (now.AddDays(-2.0)) (now.AddDays(-1.0))
    match filterAndSortCertificates now [certificate] with
    | [] -> ``👍``
    | _ -> ``💣``

[<Fact>]
let ``Not yet valid certificates should be filtered``() =
    let now = new DateTimeOffset(2023, 10, 5, 1, 2, 3, TimeSpan.Zero)

    let certificate = createCertificate (now.AddDays(1.0)) (now.AddDays(2.0))
    match filterAndSortCertificates now [certificate] with
    | [] -> ``👍``
    | _ -> ``💣``

[<Fact>]
let ``Given two valid certificates, the first certificate in the returned list must be the one with the later expiry date``() =
    let now = new DateTimeOffset(2023, 10, 5, 1, 2, 3, TimeSpan.Zero)

    let certificate1 = createCertificate (now.AddDays(-2.0)) (now.AddDays(1.0))
    let certificate2 = createCertificate (now.AddDays(-1.0)) (now.AddDays(2.0))

    match filterAndSortCertificates now [certificate1; certificate2] with
    | l when l = [certificate2; certificate1] -> ``👍``
    | _ -> ``💣``