module AppEggShellGallery.Components.ScrapedComponentProps

open LibClient
open Scraping.Types
open LibRenderDSL.Types

type Props = (* GenerateMakeFunction *) {
    FullyQualifiedName: string
    Heading:            string option // defaultWithAutoWrap None

    key: string option // defaultWithAutoWrap JsUndefined
}

let getScrapedPropsData (fullyQualifiedName: string) : ComponentProps.Data =
    let propsDataResult : Result<ScrapeResult, string> = AppEggShellGallery.ScrapedData.Props.propsDataResult

    let fields : Result<List<TaggedRecordField>, string> =
        propsDataResult
        |> Result.bind (fun propsData -> Ok propsData.Results)
        |> Result.bind (fun scrapeResult ->
            match scrapeResult.TryFind fullyQualifiedName with
            | Some value -> value |> Result.mapError (sprintf "%A")
            | None       -> Error (sprintf "Fully Qualified Name not found: %s" fullyQualifiedName)
        )
        |> Result.bind (fun taggedRecordType -> Ok taggedRecordType.Fields)

    let maybeScrapeErrors : Option<NonemptyList<ScrapeError>> =
        propsDataResult
        |> Result.toOption
        |> Option.flatMap (fun scrapeData -> NonemptyList.ofList scrapeData.Errors)

    {
        Fields            = Choice1Of2 fields
        MaybeScrapeErrors = maybeScrapeErrors
    }


type ScrapedComponentProps(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ScrapedComponentProps>("AppEggShellGallery.Components.ScrapedComponentProps", _initialProps, Actions, hasStyles = false)

and Actions(_this: ScrapedComponentProps) =
    class end

let Make = makeConstructor<ScrapedComponentProps, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
