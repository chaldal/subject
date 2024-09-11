module LibUiSubject.Services.BlobService

type BlobService (ecosystemName: string, backendUrl: string) =
    member _.ToUrl (blobId: BlobId) : string =
        $"{backendUrl}/api/v1/ecosystem/{ecosystemName}/subject{blobId.Url}"

    member this.ToDownloadUrl (blobId: BlobId) : string =
        (this.ToUrl blobId) + "?download=1"

    member this.ToDownloadUrl (blobId: BlobId, filename: string) : string =
        (this.ToDownloadUrl blobId) + $"&filename={filename}"