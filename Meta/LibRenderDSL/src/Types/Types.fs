module LibRenderDSL.Types

type TaggedRecordField =
| Regular of
    name:    string *
    theType: string

| WithDefault of
    name:         string *
    theType:      string *
    defaultValue: string

| WithDefaultAutoWrapSome of
    name:          string *
    unwrappedType: string *
    defaultValue:  string
with
    member this.Name : string =
        match this with
        | Regular                 (name, _)
        | WithDefault             (name, _, _)
        | WithDefaultAutoWrapSome (name, _, _) -> name

    member this.Type : string =
        match this with
        | Regular                 (_, theType)
        | WithDefault             (_, theType, _)
        | WithDefaultAutoWrapSome (_, theType, _) -> theType

type TaggedRecordType = {
    Name:   string
    Fields: List<TaggedRecordField>
}