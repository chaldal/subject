module ThirdParty.ImagePicker.Components.Native.ImagePickerRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open ReactXP.Components
open ThirdParty.ImagePicker.Components

open LibLang
open LibClient.RenderHelpers
open LibClient

open ThirdParty.ImagePicker.Components.Native.ImagePicker



let render(children: array<ReactElement>, props: ThirdParty.ImagePicker.Components.Native.ImagePicker.Props, estate: ThirdParty.ImagePicker.Components.Native.ImagePicker.Estate, pstate: ThirdParty.ImagePicker.Components.Native.ImagePicker.Pstate, actions: ThirdParty.ImagePicker.Components.Native.ImagePicker.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "ReactXP.Components.View"
    ReactXP.Components.Constructors.RX.View(
        children =
            [|
                (
                    if (props.ShowPreview) then
                        let __parentFQN = Some "LibClient.Components.Thumbs"
                        LibClient.Components.Constructors.LC.Thumbs(
                            ``for`` = (LibClient.Components.Thumbs.PropForFactory.Make(props.Value |> List.map (fun file -> file.ToDataUri |> LibClient.Services.ImageService.ImageSource.ofUrl))),
                            styles = ([| ImagePickerStyles.Styles.imageThumbs |])
                        )
                    else noElement
                )
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
                            let __parentFQN = Some "ThirdParty.ImagePicker.Components.Native.ReactNativeImagePicker"
                            ThirdParty.ImagePicker.Components.Constructors.ImagePicker.Native.ReactNativeImagePicker(
                                onImageSelect = (actions.SelectImage)
                            )
                            let __parentFQN = Some "ReactXP.Components.View"
                            let __currClass = "constrain-message"
                            let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                            ReactXP.Components.Constructors.RX.View(
                                ?styles = (if (not __currStyles.IsEmpty) then (ReactXP.LegacyStyles.Runtime.prepareStylesForPassingToReactXpComponent "ReactXP.Components.View" __currStyles |> Some) else None),
                                children =
                                    [|
                                        let __parentFQN = Some "LibClient.Components.LegacyText"
                                        let __currClass = "text-center"
                                        let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                        LibClient.Components.Constructors.LC.LegacyText(
                                            ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                            children =
                                                [|
                                                    match ((props.MaxFileCount, props.MaxFileSize)) with
                                                    | (Some maxFileCount, Some maxFileSize) when maxFileCount.Value > 1 ->
                                                        [|
                                                            makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}{3}{4}", "Maximum ", (maxFileCount.Value), " files each below ", (LibLifeCycleTypes.File.kBToMB maxFileSize), " MB"))
                                                        |]
                                                    | (Some maxFileCount, None            ) when maxFileCount.Value > 1 ->
                                                        [|
                                                            makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "Maximum ", (maxFileCount.Value), " files"))
                                                        |]
                                                    | (_,                 Some maxFileSize)                             ->
                                                        [|
                                                            makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "Size below ", (LibLifeCycleTypes.File.kBToMB maxFileSize), " MB"))
                                                        |]
                                                    | _ ->
                                                        [|
                                                            LibClient.EggShellReact.noElement
                                                        |]
                                                    |> castAsElementAckingKeysWarning
                                                |]
                                        )
                                    |]
                            )
                            let __parentFQN = Some "ReactXP.Components.View"
                            ReactXP.Components.Constructors.RX.View(
                                children =
                                    [|
                                        (
                                            if (props.Value.Length = 1) then
                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                let __currClass = "text-center"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                LibClient.Components.Constructors.LC.LegacyText(
                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                    children =
                                                        [|
                                                            makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "", (props.Value.Length), " file selected"))
                                                        |]
                                                )
                                            else noElement
                                        )
                                        (
                                            if (props.Value.Length > 1) then
                                                let __parentFQN = Some "LibClient.Components.LegacyText"
                                                let __currClass = "text-center"
                                                let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                LibClient.Components.Constructors.LC.LegacyText(
                                                    ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                    children =
                                                        [|
                                                            makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "", (props.Value.Length), " files selected"))
                                                        |]
                                                )
                                            else noElement
                                        )
                                    |]
                            )
                            (
                                (Option.getOrElse props.Validity.InvalidReason (Some estate.InternalValidity.InvalidReason))
                                |> Option.map
                                    (fun reason ->
                                        let __parentFQN = Some "ReactXP.Components.View"
                                        ReactXP.Components.Constructors.RX.View(
                                            children =
                                                [|
                                                    let __parentFQN = Some "LibClient.Components.LegacyText"
                                                    let __currClass = "invalid-reason"
                                                    let __currStyles = (ReactXP.LegacyStyles.Runtime.findApplicableStyles __mergedStyles __currClass)
                                                    LibClient.Components.Constructors.LC.LegacyText(
                                                        ?xLegacyStyles = (if (not __currStyles.IsEmpty) then Some __currStyles else None),
                                                        children =
                                                            [|
                                                                makeTextNode2 __parentFQN (System.String.Format("{0}", reason))
                                                            |]
                                                    )
                                                |]
                                        )
                                    )
                                |> Option.getOrElse noElement
                            )
                        |]
                )
            |]
    )
