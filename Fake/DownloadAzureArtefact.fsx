module Egg.Shell.Fake.DownloadAzureArtifact

#if !FAKE_BUILD_FSPROJ
#r "nuget: Azure.Storage.Blobs, 12.20.0"
#endif

open Azure.Storage
open Azure.Storage.Blobs
open Azure.Storage.Blobs.Models
open System.IO
open System.IO.Compression

let private makeBlobContainerClient
        (accountName:   string)
        (accountKey:    string)
        (containerName: string)
        : BlobContainerClient =
    BlobContainerClient(
        $"https://{accountName}.blob.core.windows.net/{containerName}" |> System.Uri,
        StorageSharedKeyCredential(accountName, accountKey)
    )

let private fetchMaybeLatestPackageName
        (blobContainerClient: BlobContainerClient)
        (packageNamePrefix:   string)
        : Option<string> =
    blobContainerClient.GetBlobs(BlobTraits.All, BlobStates.None, packageNamePrefix)
    |> Seq.map (fun blob -> blob.Name)
    |> Seq.sortDescending
    |> Seq.tryHead

let private deleteExistingIfDifferent
        (packageName:       string)
        (packageNamePrefix: string)
        : unit =
    Directory.EnumerateFiles "./"
    |> Seq.filter (fun filename -> filename <> packageName && filename.StartsWith packageNamePrefix && filename.EndsWith ".zip")
    |> Seq.iter File.Delete

let private downloadPackage
        (blobContainerClient: BlobContainerClient)
        (packageName:         string)
        : unit =
    let blobClient  = blobContainerClient.GetBlobClient packageName
    let blobContent = blobClient.DownloadContent()
    File.WriteAllBytes(packageName, blobContent.Value.Content.ToArray())


type Config = {
    StorageContainerName:  string
    LocalTargetPackageDir: string
    PackageNamePrefix:     string
}

let downloadAzureArtefact (config: Config) : unit =
    // NB: the account key and name is set at the storage account level, not the container level
    //     meaning all containers have the same key, therefore putting it here to save
    //     copy&paste + updating if/when we update the key
    let storageAccountName = "HIDDEN"
    let storageAccountKey  = "HIDDEN"

    let blobContainerClient =
        makeBlobContainerClient
            storageAccountName
            storageAccountKey
            config.StorageContainerName

    match fetchMaybeLatestPackageName blobContainerClient config.PackageNamePrefix with
    | None -> failwith "Failed to fetch latest package name"
    | Some packageName ->
        deleteExistingIfDifferent packageName config.PackageNamePrefix
        printfn $"Downloading package: {packageName}"
        downloadPackage blobContainerClient packageName
        printfn $"Extracting package to: {config.LocalTargetPackageDir}"
        ZipFile.ExtractToDirectory(packageName, config.LocalTargetPackageDir, overwriteFiles = true)
        printfn $"Extracted package to: {config.LocalTargetPackageDir}"
        File.Delete $"./{packageName}"
        printfn $"Deleted zip archive {packageName}"
        printfn "Done"
