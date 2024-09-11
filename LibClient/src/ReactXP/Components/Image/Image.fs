[<AutoOpen>]
module ReactXP.Components.Image

open ReactXP.Helpers
open ReactXP.Types

open LibClient
open LibClient.JsInterop

open Fable.Core.JsInterop
open Fable.React
open Fable.Core
open Browser.Types

[<StringEnum>]
type AndroidResizeMethod =
| Auto
| Resize
| Scale

[<StringEnum>]
type ResizeMode =
| Stretch
| Contain
| Cover
| Auto
| Repeat

type Dimensions = {
    width:  int
    height: int
}

type Size =
| FromParentLayout of Option<LibClient.Output.Layout>
| FromStyles
| Raw
| ImplicitRaw

let ofUrl                 = LibClient.Services.ImageService.ImageSource.ofUrl
let ofPossiblyRelativeUrl = LibClient.Services.ImageService.ImageSource.ofPossiblyRelativeUrl

let mutable private implicitRawWarnedSources: Set<LibClient.Services.ImageService.ImageSource> = Set.empty

type ReactXP.Components.Constructors.RX with
    static member Image(
        source:               LibClient.Services.ImageService.ImageSource,
        ?size:                Size,
        ?headers:             Headers,
        ?accessibilityLabel:  string,
        ?resizeMode:          ResizeMode,
        ?androidResizeMethod: AndroidResizeMethod,
        ?title:               string,
        ?onLoad:              Dimensions -> unit,
        ?onError:             ErrorEvent -> unit,
        ?styles:              array<ReactXP.Styles.FSharpDialect.ViewStyles>,
        ?xLegacyStyles:       List<ReactXP.LegacyStyles.RuntimeStyles>
    ) =
        // NOTE TODO we do not currently handle cover/stretch properly in the optimized URL calculations,
        // so there's a chance we'll be requesting undersized images in some circumstances.
        let maybeUpdatedSource =
            match defaultArg size (Size.ImplicitRaw) with
            | Raw -> source.Url |> Some
            | ImplicitRaw ->
                if not (implicitRawWarnedSources.Contains source) then
                    implicitRawWarnedSources <- implicitRawWarnedSources.Add source
                    Log.Warn ("An Image is used without Size being specified. This will render, but you should make an explicit choice. Source: {source}", source)

                source.Url |> Some
            | FromStyles ->
                let maybeWidth =
                    match styles with
                    | None ->
                        Log.Warn ("An Image is used with Size specified as FromStyles, but props?style is null. Source: {source}", source)
                        None
                    | Some nonNull ->
                        (ReactXP.LegacyStyles.Runtime.extractReactXpStyleValue "width" (nonNull :> obj :?> array<obj>)) :> obj :?> Option<int>

                LibClient.ServiceInstances.services().Image.MakeOptimizedUrl source maybeWidth |> Some
            | FromParentLayout maybeLayout ->
                match maybeLayout with
                | None        -> None
                | Some layout -> LibClient.ServiceInstances.services().Image.MakeOptimizedUrl source (Some layout.Width) |> Some

        match maybeUpdatedSource with
        | None -> noElement
        | Some updatedSource ->
            let __props = createEmpty

            __props?source              <- updatedSource
            __props?headers             <- headers
            __props?accessibilityLabel  <- accessibilityLabel
            __props?resizeMode          <- resizeMode
            __props?androidResizeMethod <- androidResizeMethod
            __props?title               <- title
            __props?onLoad              <- onLoad
            __props?onError             <- onError
            __props?style               <- styles

            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles

            Fable.React.ReactBindings.React.createElement(
                ReactXPRaw?Image,
                __props,
                [||]
            )
