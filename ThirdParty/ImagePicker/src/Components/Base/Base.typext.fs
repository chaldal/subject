module ThirdParty.ImagePicker.Components.Base

open LibClient
open LibLifeCycleTypes.File
open ReactXP.Styles

type SelectionMode = LibClient.Components.Input.File.SelectionMode
let ReplacedExisting = SelectionMode.ReplacedExisting
let AppendToExisting = SelectionMode.AppendToExisting

type Props = (* GenerateMakeFunction *) {
    Value:         list<File>
    Validity:      InputValidity
    OnChange:      Result<list<File>, string> -> unit
    MaxFileCount:  Positive.PositiveInteger option // defaultWithAutoWrap None
    MaxFileSize:   int<KB> option                  // defaultWithAutoWrap None
    ShowPreview:   bool                            // default true
    SelectionMode: SelectionMode                   // default ReplacedExisting
    styles:        array<ViewStyles> option // defaultWithAutoWrap None
    key: string option // defaultWithAutoWrap JsUndefined
}

type Base(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Base>("ThirdParty.ImagePicker.Components.Base", _initialProps, Actions, hasStyles = true)

and Actions(_this: Base) =
    class end

let Make = makeConstructor<Base, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate