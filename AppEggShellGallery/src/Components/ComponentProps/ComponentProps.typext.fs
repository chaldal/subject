module AppEggShellGallery.Components.ComponentProps

open LibClient
open Scraping.Types
open LibRenderDSL.Types

type XmlParam = {
    Name:        string
    Type:        string
    Default:     Option<string>
    Description: Option<string>
}

type Data = {
    Fields:            Choice<Result<List<TaggedRecordField>, string>, List<XmlParam>>
    MaybeScrapeErrors: Option<NonemptyList<ScrapeError>>
} with
    member this.FieldsWithoutKey : Result<List<XmlParam>, string> =
        match this.Fields with
        | Choice1Of2 fieldsRes ->
            fieldsRes
            |> Result.map (fun fields ->
                fields
                |> List.filterNot (fun field -> field.Name = "key")
                |> List.map (fun taggedRecordField ->
                    match taggedRecordField with
                    | TaggedRecordField.Regular (name, theType) ->
                        {
                            Name        = name
                            Type        = theType
                            Default     = None
                            Description = None
                        }
                    | TaggedRecordField.WithDefault (name, theType, theDefault)
                    | TaggedRecordField.WithDefaultAutoWrapSome (name, theType, theDefault) ->
                        {
                            Name        = name
                            Type        = theType
                            Default     = Some theDefault
                            Description = None
                        }
                )
            )
        | Choice2Of2 xmlParams ->
            xmlParams
            |> List.filterNot (fun param -> param.Name = "key")
            |> Ok

type Props = (* GenerateMakeFunction *) {
    Heading: string option // defaultWithAutoWrap None
    Data:    Data

    key: string option // defaultWithAutoWrap JsUndefined
}

type ComponentProps(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ComponentProps>("AppEggShellGallery.Components.ComponentProps", _initialProps, Actions, hasStyles = true)

and Actions(_this: ComponentProps) =
    class end

let Make = makeConstructor<ComponentProps, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate