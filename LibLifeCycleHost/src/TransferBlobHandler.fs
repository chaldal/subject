[<AutoOpen>]
module LibLifeCycleHost.TransferBlobHandler

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.DataProtection.KeyManagement

type ITransferBlobHandler =
    abstract member Name: string
    
    abstract member StoreBlobsForTransfer : Map<Guid, byte[]>  -> Task<unit>
    
    abstract member GetTransferBlobs :   Set<Guid> -> Task<Map<Guid, byte[]>>
    
    abstract member DeleteTransferBlobs: Set<Guid> -> Task<unit>