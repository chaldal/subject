[<AutoOpen>]
module LibClient.Components.Thumbs

open Fable.React

open LibClient
open LibClient.Services.ImageService

open ReactXP.Styles

module LC =
    module Thumbs =
        type For<'T when 'T : comparison> =
        | Values of list<'T> * ('T -> ImageSource)
        with
            member this.Items : list<'T> =
                match this with
                | Values (values, _) -> values

            member this.ToSource : 'T -> ImageSource =
                match this with
                | Values (_, toSource) -> toSource

        type For =
            static member Of(sources: list<ImageSource>) : For<ImageSource> = Values (sources, id)

            static member Of(values: list<'T>, toSource: 'T -> ImageSource) : For<'T> = Values (values, toSource)


open LC.Thumbs

// TODO: delete after RenderDSL migration
type PropForFactory =
    static member Make (sources: list<ImageSource>) : For<ImageSource> = For.Of sources
    static member Make (values: list<'T>, toSource: 'T -> ImageSource) : For<'T> = For.Of(values, toSource)

[<RequireQualifiedAccess>]
module private Styles =
    let notLastThumb =
        makeViewStyles {
            marginRight 8 // TODO make themeable
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Thumbs<'T when 'T: comparison>(
            ``for``: For<'T>,
            ?selected: Set<'T>,
            ?onPress: 'T -> uint32 -> ReactEvent.Action -> unit,
            ?styles: array<ViewStyles>,
            ?key: string
        ) : ReactElement =
        key |> ignore

        let selected = defaultArg selected Set.empty

        LC.ItemList(
            styles = (styles |> Option.defaultValue [||]),
            items = ``for``.Items,
            style = Style.Horizontal,
            whenEmpty = (WhenEmpty.Message "No Images"),
            whenNonempty =
                fun (items: seq<'T>) ->
                    let itemsLength = items |> Seq.length

                    element {
                        items
                        |> Seq.mapi (fun index item ->
                            let isLastThumb = index = (itemsLength - 1)

                            LC.Thumb(
                                ``for`` = LC.Thumb.For.Of(item, ``for``.ToSource),
                                styles =
                                    [|
                                        if not isLastThumb then
                                            Styles.notLastThumb
                                    |],
                                isSelected = (selected.Contains item),
                                ?onPress = (onPress |> Option.map (fun onPress -> onPress item (uint32 index)))
                            )
                        )
                    }
        )