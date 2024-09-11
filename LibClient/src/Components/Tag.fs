[<AutoOpen>]
module LibClient.Components.Tag

open Fable.React

open LibClient
open LibClient.Responsive

open ReactXP.Components
open ReactXP.Styles

module LC =
    module Tag =
        // TODO generalize this as a state of all OnPressable things
        type State =
        | ViewOnly
        | Actionable of OnPress: (ReactEvent.Action -> unit)
        | InProgress
        | Disabled

        type TagTheme = {
            TextColor: Color
            BackgroundColor: Color
        }

        type TagsTheme = {
            Selected: TagTheme
            Unselected: TagTheme
        }

        type SizeTheme = {
            FontSize: int
            PaddingHorizontal: int
            PaddingVertical: int
        }

        type SizesTheme = {
            Desktop: SizeTheme
            Handheld: SizeTheme
        }

        type Theme = {
            Tags: TagsTheme
            Sizes: SizesTheme
        }
        with
            member this.TagAndSizeTheme (screenSize: ScreenSize) (isSelected: bool): TagTheme * SizeTheme =
                let tagTheme =
                    if isSelected then
                        this.Tags.Selected
                    else
                        this.Tags.Unselected
                let sizeTheme =
                    match screenSize with
                    | ScreenSize.Desktop -> this.Sizes.Desktop
                    | ScreenSize.Handheld -> this.Sizes.Handheld
                (tagTheme, sizeTheme)

open LC.Tag

[<RequireQualifiedAccess>]
module private Styles =
    // Limited to states that actually affect the theme, otherwise we get style leak warnings.
    [<RequireQualifiedAccess>]
    type ThemeState =
    | Actionable
    | Disabled
    | Other

    let view =
        makeViewStyles {
            // TODO it's wrong to specify margin internally — LC.Tags should specify margin
            marginHorizontal 6
            marginVertical 6
            borderRadius 20
            Cursor.Default
        }

    let labelBlock =
        makeViewStyles {
            FlexDirection.Row
            JustifyContent.Center
            AlignItems.Center
        }

    let labelText =
        makeTextStyles {
            FontWeight.W300
        }

    let spinnerBlock =
        makeViewStyles {
            Position.Absolute
            trbl 0 0 0 0
            AlignItems.Center
            JustifyContent.Center
            backgroundColor (Color.WhiteAlpha 0.5)
        }

    let textTheme =
        TextStyles.Memoize(
            fun (theme: Theme) (screenSize: ScreenSize) (isSelected: bool) ->
                let tagTheme, sizeTheme = theme.TagAndSizeTheme screenSize isSelected

                makeTextStyles {
                    color tagTheme.TextColor
                    fontSize sizeTheme.FontSize
                }
        )

    let viewTheme =
        ViewStyles.Memoize(
            fun (theme: Theme) (screenSize: ScreenSize) (isSelected: bool) (isHovered: bool) (isDepressed: bool) (state: ThemeState) ->
                let tagTheme, sizeTheme = theme.TagAndSizeTheme screenSize isSelected

                makeViewStyles {
                    paddingHorizontal sizeTheme.PaddingHorizontal
                    paddingVertical sizeTheme.PaddingVertical

                    backgroundColor tagTheme.BackgroundColor

                    if isHovered then
                        shadow (Color.BlackAlpha 0.2) 5 (0, 3)
                    else if isDepressed then
                        shadow (Color.BlackAlpha 0.2) 3 (0, 0)
                        top 1

                    match state with
                    | ThemeState.Disabled ->
                        opacity 0.5
                    | ThemeState.Actionable _ ->
                        Cursor.Pointer
                    | ThemeState.Other ->
                        Noop
                }
        )

type private State with
    member private this.ToThemeState() =
        match this with
        | State.Actionable _ -> Styles.ThemeState.Actionable
        | State.Disabled -> Styles.ThemeState.Disabled
        | _ -> Styles.ThemeState.Other

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Tag(
            text: string,
            ?state: State,
            ?isSelected: bool,
            ?theme: Theme -> Theme,
            ?styles: array<ViewStyles>,
            ?key: string
        ) : ReactElement =
        key |> ignore

        let theTheme = Themes.GetMaybeUpdatedWith theme
        let state = defaultArg state State.ViewOnly
        let isSelected = defaultArg isSelected false
        let styles = defaultArg styles [||]

        LC.With.ScreenSize(
            fun screenSize ->
                LC.Pointer.State(
                    fun pointerState ->
                        let isHovered = pointerState.IsHovered && (not pointerState.IsDepressed)
                        let isDepressed = pointerState.IsDepressed

                        RX.View(
                            styles =
                                [|
                                    Styles.view
                                    Styles.viewTheme theTheme screenSize isSelected isHovered isDepressed (state.ToThemeState())
                                    yield! styles
                                |],
                            children =
                                elements {
                                    RX.View(
                                        styles = [| Styles.labelBlock |],
                                        children =
                                            elements {
                                                LC.Text(
                                                    text,
                                                    styles =
                                                        [|
                                                            Styles.labelText
                                                            Styles.textTheme theTheme screenSize isSelected
                                                        |]
                                                )
                                            }
                                    )

                                    match state with
                                    | State.Actionable onPress ->
                                        LC.TapCapture(
                                            key = "tap capture",
                                            pointerState = pointerState,
                                            onPress = onPress
                                        )
                                    | State.InProgress ->
                                        RX.View(
                                            styles = [| Styles.spinnerBlock |],
                                            children =
                                                elements {
                                                    RX.ActivityIndicator(
                                                        color = "#aaaaaa",
                                                        size = Size.Tiny
                                                    )
                                                }
                                        )
                                    | _ ->
                                        noElement
                                }
                        )
                )
        )
