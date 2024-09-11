module RenderDSLServer.Files

type FileInfo      = System.IO.FileInfo
type DocumentStore = LSP.DocumentStore

let private readConfigFromFileSystem (path: string) : Async<Option<string>> = async {
    match System.IO.File.Exists path with
    | false -> return None
    | true  ->
        let! content = Async.AwaitTask (System.IO.File.ReadAllTextAsync path)
        return Some content
}

let getTextFromStoreOrFilesystemIfNotOpen (documentStore: DocumentStore) (path: string) : Async<Option<string>> =
    match documentStore.GetText (FileInfo path) with
    | Some contentFromDocStore -> async { return Some contentFromDocStore }
    | None -> readConfigFromFileSystem path
