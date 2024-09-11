module LibClient.Components.Input.ImageRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open LibClient.Components

open LibClient
open LibClient.RenderHelpers
open LibClient.Icons
open LibClient.Chars
open LibClient.ColorModule
open LibClient.LocalImages
open LibClient.Responsive

open LibClient.Components.Input.Image



let render(children: array<ReactElement>, props: LibClient.Components.Input.Image.Props, estate: LibClient.Components.Input.Image.Estate, pstate: LibClient.Components.Input.Image.Pstate, actions: LibClient.Components.Input.Image.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    let __currClass = (System.String.Format("{0}{1}{2}", "view ", (TopLevelBlockClass), ""))
    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
    ReactXP.Components.Constructors.RX.View(
        ?styles =
            (
                let __currProcessedStyles = if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "%s" __currStyles) else [||]
                match props.styles with
                | Some styles ->
                    Array.append __currProcessedStyles styles |> Some
                | None -> Some __currProcessedStyles
            ),
        children =
            [|
                (
                    if (props.ShowPreview) then
                        let __parentFQN = Some "LibClient.Components.Thumbs"
                        LibClient.Components.Constructors.LC.Thumbs(
                            onPress = (fun _ index _ -> actions.ToggleSelectedFilesForRemoval index),
                            ``for`` = (LibClient.Components.Thumbs.PropForFactory.Make(props.Value |> List.map (fun file -> file.ToDataUri |> LibClient.Services.ImageService.ImageSource.ofUrl))),
                            selected = (estate.SelectedFiles),
                            styles = ([| ImageStyles.Styles.imageThumbs |])
                        )
                    else noElement
                )
                (
                    if (estate.SelectedIndicesForRemoval.IsNonempty) then
                        let __parentFQN = Some "LibClient.Components.TextButton"
                        LibClient.Components.Constructors.LC.TextButton(
                            state = (LibClient.Components.TextButton.PropStateFactory.MakeLowLevel (LC.TextButton.Actionable actions.RemoveSelected)),
                            label = ("Remove Selected")
                        )
                    else noElement
                )
                let __parentFQN = Some "LibClient.Components.Input.File"
                let __currClass = "image-input"
                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                LibClient.Components.Constructors.LC.Input.File(
                    onChange = (props.OnChange),
                    selectionMode = (props.SelectionMode),
                    acceptedTypes = ([ LibClient.Components.Input.File.AnyImageFile ] |> Set.ofSeq),
                    ?maxFileSize = (props.MaxFileSize),
                    ?maxFileCount = (props.MaxFileCount),
                    validity = (props.Validity),
                    value = (props.Value),
                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None)
                )
            |]
    )
