[<AutoOpen>]
module AppEggShellGallery.Components.Route_TinyGuid

open Fable.React
open LibClient
open LibClient.Components
open LibRouter.Components
open ReactXP.Components
open ReactXP.Styles

type Ui.Route with
    [<Component>]
    static member TinyGuid () : ReactElement = element {
        let guidState = Hooks.useState<Option<NonemptyString>> None
        let maybeGuid =
            guidState.current
            |> Option.map NonemptyString.value
            |> Option.flatMap System.Guid.ParseOption

        let tinyState = Hooks.useState<Option<NonemptyString>> None
        let maybeTiny =
            tinyState.current
            |> Option.map NonemptyString.value
            |> Option.flatMap System.Guid.TryFromTinyUuid

        LC.SetPageMetadata (title = "TinyGuid")
        LR.Route (
            children = [|
                LC.Centered (
                    child = LC.Constrained (
                        maxWidth = 500,
                        child = LC.Column (crossAxisAlignment = LC.CrossAxisAlignment.Stretch, children = [|
                            LC.Sized (height = 40)
                            LC.Row [|
                                LC.Input.Text (
                                    styles   = [|Styles.Expand|],
                                    label    = "Regular Guid",
                                    value    = guidState.current,
                                    onChange = guidState.update,
                                    validity = Valid
                                )
                                LC.Sized (width = 10)
                                RX.View (styles = [|Styles.Expand|], children = [|
                                    LC.Text (
                                        value  = match maybeGuid with Some guid -> guid.ToTinyUuid () | None -> ""
                                    )
                                |])
                            |]
                            LC.Sized (height = 40)
                            LC.Row [|
                                LC.Input.Text (
                                    styles   = [|Styles.Expand|],
                                    label    = "Tiny Uuid",
                                    value    = tinyState.current,
                                    onChange = tinyState.update,
                                    validity = Valid
                                )
                                LC.Sized (width = 10)
                                RX.View (styles = [|Styles.Expand|], children = [|
                                    LC.Text (
                                        value = match maybeTiny with Some tiny -> tiny.ToString () | None -> ""
                                    )
                                |])
                            |]
                        |])
                    )
                )
            |]
        )
    }

and private Styles() =
    static member val Expand = makeViewStyles {
        flex 1
        flexBasis 0
    }

