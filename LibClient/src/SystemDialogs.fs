[<AutoOpen>]
module LibClient.SystemDialogs

open LibClient.Services.ImageService
open ReactXP.Components.Image

type SystemDialog =
| ConfirmCustom     of MaybeHeading: Option<string> * Details: string * Buttons: List<LibClient.Components.Dialog.Confirm.Button>
| Confirm           of MaybeHeading: Option<string> * Details: string * CancelLabel: string * OkLabel: string * OnResult: (bool -> unit)
| ConfirmAsync      of MaybeHeading: Option<string> * Details: string * CancelLabel: string * OkLabel: string * OnConfirm: (unit -> Async<Result<unit, string>>)
| Alert             of MaybeHeading: Option<string> * Details: string
| Prompt            of MaybeHeading: Option<string> * Details: string * OnResult: (Option<NonemptyString> -> unit)
| ImageViewer       of list<ImageSource> * InitialIndex: uint32
| ImageViewerCustom of list<ImageSource> * InitialIndex: uint32 * ResizeMode
