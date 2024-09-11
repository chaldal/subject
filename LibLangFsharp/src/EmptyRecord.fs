namespace LibLang

[<AutoOpen>]
module EmptyRecord =
    // F# does not allow empty records, which makes stubbing a real bitch,
    // breaking the dev style of "get the structure in place, then fill in the details".
    type EmptyRecordType = {
        __emptyRecordPlaceholder: Option<unit>
    }
    let EmptyRecord = { __emptyRecordPlaceholder = None }

    let EmptyRecordCast<'TypeAlias> = EmptyRecord :> obj :?> 'TypeAlias
