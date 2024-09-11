module Scraping.Types

open LibRenderDSL.Types

type ScrapeError =
| DirectoryError             of string
| FileReadError              of string
| FullyQualifiedNameNotFound of string
| PropsError                 of PropsError

and PropsError =
| NoProps
| MultipleProps
| ExtractError of string

type ScrapeResult = {
    Results: Map<string, Result<TaggedRecordType, PropsError>>
    Errors:  List<ScrapeError>
} with
    static member Empty : ScrapeResult = {
        Results = Map.empty
        Errors  = []
    }

    member this.AddError (e: ScrapeError) : ScrapeResult =
        { this with Errors = e :: this.Errors }


type FilePath = FilePath of string

[<RequireQualifiedAccess>]
type SnippetError =
| FileRead of string
| Decode   of string

type Snippet = {
  Key:         string
  Prefix:      string
  Description: string
  Body:        List<string>
  Scope:       string
}
