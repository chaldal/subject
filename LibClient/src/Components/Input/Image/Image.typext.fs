module LibClient.Components.Input.Image

open LibClient
open LibLifeCycleTypes.File
open LibClient.Services.ImageService
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

type Estate = {
    SelectedFiles:             Set<LibClient.Services.ImageService.ImageSource>
    SelectedIndicesForRemoval: Set<uint32>
}

type Image(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Image>("LibClient.Components.Input.Image", _initialProps, Actions, hasStyles = true)

    override this.GetInitialEstate (_: Props) : Estate = {
        SelectedIndicesForRemoval = Set.empty
        SelectedFiles             = Set.empty
    }

    member this.DeleteFileFromIndices (selectedIndices: Set<uint32>) : list<File> =
        this.props.Value
        |> List.indexed
        |> List.filter (fun (index, _) -> not (selectedIndices |> Set.contains (uint32 index)))
        |> List.map snd

and Actions(this: Image) =
    member _this.ToggleSelectedFilesForRemoval (index: uint32): unit =
        this.SetEstate (fun estate _ ->
            let newSelectedIndicesForRemoval = estate.SelectedIndicesForRemoval.Toggle index
            { estate with
                SelectedIndicesForRemoval = newSelectedIndicesForRemoval
                SelectedFiles =
                    this.props.Value
                    |> List.except (this.DeleteFileFromIndices newSelectedIndicesForRemoval)
                    |> List.map (fun file -> file.ToDataUri |> LibClient.Services.ImageService.ImageSource.ofUrl)
                    |> Set.ofList
            }
        )

    member _this.RemoveSelected (_: ReactEvent.Action) : unit =
        this.SetEstate (fun estate _ ->
            this.props.OnChange ((this.DeleteFileFromIndices estate.SelectedIndicesForRemoval) |> Ok)
            { estate with SelectedIndicesForRemoval = Set.empty; SelectedFiles = Set.empty }
        )

let Make = makeConstructor<Image, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
