[<AutoOpen>]
module LibClient.Components.AsyncData

open Fable.React

open LibClient
open LibClient.Components

open ReactXP.Components
open ReactXP.Styles

[<RequireQualifiedAccess>]
module private Styles =
    // In cases where there are no other components being added to the tree (e.g. when Fetching None), we inline the
    // activity indicator to ensure it takes up the space it requires.
    let inlinedActivityIndicator =
        makeViewStyles {
            FlexDirection.Row
            JustifyContent.Center
            AlignItems.Center
        }

    // In cases where there are other components being added to the tree (e.g. when Fetching Some), we overlay the
    // activity indicator above those components (an implication being that the activity indicator is constrained
    // to the space those components occupy, which means it may get clipped).
    let overlaidActivityIndicator =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
            FlexDirection.Row
            JustifyContent.Center
            AlignItems.Center
            backgroundColor (Color.WhiteAlpha 0.5)
        }

[<RequireQualifiedAccess>]
type private HelperComponents =
    [<Component>]
    static member ActivityIndicator(overlay: bool) : ReactElement =
        RX.View(
            key = "activityIndicator",
            children =
                elements {
                    RX.ActivityIndicator(
                        color = "#aaaaaa"
                    )
                },
            styles =
                [|
                    if overlay then
                        Styles.overlaidActivityIndicator
                    else
                        Styles.inlinedActivityIndicator
                |]
        )

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member AsyncData<'T>(
            data: LibClient.AsyncDataModule.AsyncData<'T>,
            whenAvailable: 'T -> ReactElement,
            ?whenUninitialized: unit -> ReactElement,
            ?whenFetching: Option<'T> -> ReactElement,
            ?whenFailed: AsyncDataFailure -> ReactElement,
            ?whenUnavailable: unit -> ReactElement,
            ?whenAccessDenied: unit -> ReactElement,
            ?whenElse: unit -> ReactElement
        ) : ReactElement =
        // It is vitally important that we keep the child stable in terms of its identity in the React tree, so that React does not
        // unnecessarily remount it in response to changes to data. Without this wrapping, a transition from Fetching to Available
        // can result in the child being remounted because the React tree composed for Fetching has a different depth than that
        // for Available.
        let wrapWithStableKey (child: ReactElement) =
            LC.Fragment(
                children =
                    elements {
                        child
                    },
                key = "child"
            )

        element {
            match data with
            | AsyncData.Available data ->
                data |> whenAvailable |> wrapWithStableKey

            | AsyncData.Unavailable ->
                match whenUnavailable, whenElse with
                | None, None ->
                    LC.UiText "Not available"
                | Some whenUnavailable, _ ->
                    () |> whenUnavailable |> wrapWithStableKey
                | None, Some whenElse ->
                    () |> whenElse |> wrapWithStableKey

            | AsyncData.Uninitialized ->
                match whenUninitialized, whenElse with
                | None, None ->
                    HelperComponents.ActivityIndicator(overlay = false)
                | Some whenUninitialized, _ ->
                    () |> whenUninitialized |> wrapWithStableKey
                | None, Some whenElse ->
                    () |> whenElse |> wrapWithStableKey

            | AsyncData.AccessDenied ->
                match whenAccessDenied, whenElse with
                | None, None ->
                    LC.UiText "Access denied"
                | Some whenAccessDenied, _ ->
                    () |> whenAccessDenied |> wrapWithStableKey
                | None, Some whenElse ->
                    () |> whenElse |> wrapWithStableKey

            | AsyncData.WillStartFetchingSoonHack ->
                match whenFetching, whenElse with
                | None, None ->
                    HelperComponents.ActivityIndicator(overlay = false)
                | Some whenFetching, _ ->
                    None |> whenFetching |> wrapWithStableKey
                | None, Some whenElse ->
                    () |> whenElse |> wrapWithStableKey

            | AsyncData.Fetching maybeOldData ->
                match whenFetching, whenElse, maybeOldData with
                | None, None, None ->
                    HelperComponents.ActivityIndicator(overlay = false)
                | Some whenFetching, _, _ ->
                    maybeOldData |> whenFetching |> wrapWithStableKey
                | None, Some whenElse, _ ->
                    () |> whenElse |> wrapWithStableKey
                | None, None, Some oldData ->
                    oldData |> whenAvailable |> wrapWithStableKey
                    HelperComponents.ActivityIndicator(overlay = true)

            | AsyncData.Failed error ->
                match whenFailed, whenElse with
                | None, None ->
                    // error boundaries up the tree should catch this and deal with it as appropriate
                    raise (AsyncDataException error)
                | Some whenFailed, _ ->
                    error |> whenFailed |> wrapWithStableKey
                | None, Some whenElse ->
                    () |> whenElse |> wrapWithStableKey
        }

