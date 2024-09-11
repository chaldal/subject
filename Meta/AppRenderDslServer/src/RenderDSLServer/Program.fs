module RenderDSLServer.Program

open LSP.Log
open System
open System.IO
open LSP
open LSP.Types
open Session

type Position with
    static member Beginning : Position =
        {
            line      = 1
            character = 1
        }

type Range with
    static member Beginning : Range =
        {
            start    = Position.Beginning
            ``end``  = Position.Beginning
        }

type Server(client: ILanguageClient) =
    let docs = DocumentStore()

    let mutable session: Session = emptySession

    /// Send diagnostics to the client
    let publishErrors(file: FileInfo, errors: Diagnostic list) =
        client.PublishDiagnostics({uri=Uri("file://" + file.FullName); diagnostics=errors})

    /// Defer initialization operations until Initialized() is called,
    /// so that the client-side code int client/extension.ts starts running immediately
    let mutable deferredInitialize = async { () }

    let maybeGetTagName (p: TextDocumentPositionParams) : Option<string> =
        let file = FileInfo(p.textDocument.uri.LocalPath)

        docs.GetText file
        |> Option.flatMap (XML.getTagNameAtPosition p)

    interface ILanguageServer with
        member this.Initialize(p: InitializeParams) =
            async {
                match p.rootUri with
                | Some rootUri ->
                    dprintfn "Add workspace root %s" rootUri.LocalPath
                    deferredInitialize <- async {
                        let! initializedSession = initializeSession rootUri.LocalPath docs
                        session <- initializedSession
                        dprintfn "Loaded session: %O" session
                    }
                | _ -> dprintfn "No root URI in initialization message %A" p

                return {
                    capabilities =
                        { defaultServerCapabilities with
                            hoverProvider           = true
                            completionProvider      = Some({ resolveProvider = true; triggerCharacters = ['.'] })
                            signatureHelpProvider   = Some({ triggerCharacters = ['('; ','] })
                            documentSymbolProvider  = true
                            codeLensProvider        = Some({ resolveProvider = true })
                            workspaceSymbolProvider = true
                            definitionProvider      = true
                            referencesProvider      = true
                            renameProvider          = true
                            textDocumentSync        =
                                { defaultTextDocumentSyncOptions with
                                    openClose = true
                                    save      = Some({ includeText = false })
                                    change    = TextDocumentSyncKind.Incremental
                                }
                        }
                }
            }

        member this.Initialized(): Async<unit> =
            deferredInitialize

        member this.Shutdown(): Async<unit> =
            async { () }

        member this.DidChangeConfiguration(p: DidChangeConfigurationParams): Async<unit> =
            async {
                dprintfn "New configuration %s" (p.ToString())
            }

        member this.DidOpenTextDocument(p: DidOpenTextDocumentParams): Async<unit> =
            async {
                let file = FileInfo(p.textDocument.uri.LocalPath)
                // Store text in docs
                docs.Open(p)
            }

        member this.DidChangeTextDocument(p: DidChangeTextDocumentParams): Async<unit> =
            async {
                let file = FileInfo(p.textDocument.uri.LocalPath)
                docs.Change(p)
            }

        member this.WillSaveTextDocument(p: WillSaveTextDocumentParams): Async<unit> = QQQ

        member this.WillSaveWaitUntilTextDocument(p: WillSaveTextDocumentParams): Async<TextEdit list> = QQQ

        member this.DidSaveTextDocument(p: DidSaveTextDocumentParams): Async<unit> =
            async {
                Noop
            }

        member this.DidCloseTextDocument(p: DidCloseTextDocumentParams): Async<unit> =
            async {
                let file = FileInfo(p.textDocument.uri.LocalPath)
                docs.Close(p)
                // Only show errors for open files
                publishErrors(file, [])
            }

        member this.DidChangeWatchedFiles(p: DidChangeWatchedFilesParams): Async<unit> =
            async {
                Noop
            }

        member this.Completion(p: TextDocumentPositionParams): Async<CompletionList option> = QQQ

        member this.Hover(p: TextDocumentPositionParams): Async<Hover option> = async {
            match maybeGetTagName p with
            | None -> return None
            | Some tagName ->
                let! contents = Hovers.getHoverContent docs session tagName
                return Some {
                    contents = contents
                    range    = None
                }
        }

        // Add documentation to a completion item
        // Generating documentation is an expensive step, so we want to defer it until the user is actually looking at it
        member this.ResolveCompletionItem(p: CompletionItem): Async<CompletionItem> = QQQ

        member this.SignatureHelp(p: TextDocumentPositionParams): Async<SignatureHelp option> = QQQ

        member this.GotoDefinition(p: TextDocumentPositionParams): Async<Location list> = async {
            match maybeGetTagName p with
            | None -> return []
            | Some tagName ->
                let result = resultful {
                    let! componentName = session.GetComponentName tagName
                    let! filename = session.GetFilenameForComponent componentName
                    return [{
                        range = Range.Beginning
                        uri   = System.Uri(filename)
                    }]
                }

                match result with
                | Ok locationList -> return locationList
                | Error e ->
                    dprintfn "Got an error trying to goto: %s" e
                    return []
        }

        member this.FindReferences(p: ReferenceParams): Async<Location list> = QQQ

        member this.DocumentHighlight(p: TextDocumentPositionParams): Async<DocumentHighlight list> = QQQ

        member this.DocumentSymbols(p: DocumentSymbolParams): Async<SymbolInformation list> = async {
            return []
        }

        member this.WorkspaceSymbols(p: WorkspaceSymbolParams): Async<SymbolInformation list> = QQQ

        member this.CodeActions(p: CodeActionParams): Async<Command list> = QQQ

        member this.CodeLens(p: CodeLensParams): Async<List<CodeLens>> = async {
            return []
        }

        member this.ResolveCodeLens(p: CodeLens): Async<CodeLens> = QQQ

        member this.DocumentLink(p: DocumentLinkParams): Async<DocumentLink list> = QQQ

        member this.ResolveDocumentLink(p: DocumentLink): Async<DocumentLink> = QQQ

        member this.DocumentFormatting(p: DocumentFormattingParams): Async<TextEdit list> = QQQ

        member this.DocumentRangeFormatting(p: DocumentRangeFormattingParams): Async<TextEdit list> = QQQ

        member this.DocumentOnTypeFormatting(p: DocumentOnTypeFormattingParams): Async<TextEdit list> = QQQ

        member this.Rename(p: RenameParams): Async<WorkspaceEdit> = QQQ

        member this.ExecuteCommand(p: ExecuteCommandParams): Async<unit> = QQQ

        member this.DidChangeWorkspaceFolders(p: DidChangeWorkspaceFoldersParams): Async<unit> = QQQ

[<EntryPoint>]
let main(argv: array<string>): int =
    let read = new BinaryReader(Console.OpenStandardInput())
    let write = new BinaryWriter(Console.OpenStandardOutput())
    let serverFactory(client) = Server(client) :> ILanguageServer
    dprintfn "Listening on stdin"
    try
        LanguageServer.connect(serverFactory, read, write)
        0 // return an integer exit code
    with e ->
        dprintfn "Exception in language server %O" e
        1
