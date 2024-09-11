[<AutoOpen>]
module LibClient.Components.ItemList

open Fable.React

open LibClient
open LibClient.Icons
open LibClient.Components

open ReactXP.Styles
open ReactXP.Components

type WhenEmpty =
| Children of ReactElement
| Message  of string

type Style =
| Raw
| Responsive of DesktopCardAlignment: HorizontalAlignment
| Horizontal


[<RequireQualifiedAccess>]
type SeeAll =
| Default  of (ReactEvent.Action -> unit)
| Children of ReactElement


type SeeAllTheme = {
    Height:         int
    MarginVertical: int
}

module LC =
    module ItemList =
        type Theme = {
            SeeAll: SeeAllTheme
        }

type private Helpers =
    [<Component>]
    static member SeeAll (seeAll: SeeAll, theme: SeeAllTheme) : ReactElement =
        match seeAll with
        | SeeAll.Default handler ->
            RX.View (styles = [|HelperStyles.SeeAllContainer; theme.SeeAll|], children = (elements {
                RX.View (styles = [|HelperStyles.SeeAllIconContainer|], children = (elements {
                    LC.Icon (styles = [|HelperStyles.SeeAllIcon|], icon = Icon.ChevronRight)
                }))
                RX.View (styles = [|HelperStyles.Label|], children = (elements {
                    LC.Text "See All"
                }))
                LC.TapCapture (onPress = handler)
            }))
        | SeeAll.Children children ->
            children

and private HelperStyles() =
    static member val SeeAllContainer = makeViewStyles {
        AlignItems.Center
        FlexDirection.Column
        JustifyContent.Center
        width 106
    }
    static member val SeeAllIconContainer = makeViewStyles {
        AlignItems.Center
        JustifyContent.Center
        size 44 44
    }
    static member val SeeAllIcon = makeTextStyles {
        color    (Color.Grey "cc")
        fontSize 50
    }
    static member val Label = makeViewStyles {
        marginTop 8
    }
    static member val LabelText = makeTextStyles {
        TextAlign.Center
        fontSize 15
    }

and SeeAllTheme with
    member this.SeeAll = makeViewStyles {
        height         this.Height
        marginVertical this.MarginVertical
    }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member ItemList<'T> (
        items:        seq<'T>,
        whenNonempty: seq<'T> -> ReactElement,
        style:        Style,
        ?whenEmpty:   WhenEmpty,
        ?seeAll:      SeeAll,
        ?styles:      array<ViewStyles>,
        ?theme:       LC.ItemList.Theme -> LC.ItemList.Theme
    ) : ReactElement = element {
        let whenEmpty = whenEmpty |> Option.getOrElse (WhenEmpty.Message "No Items")
        let theTheme  = Themes.GetMaybeUpdatedWith theme
        let externalStyles =
            styles
            |> Option.getOrElse Array.empty

        RX.View (styles = externalStyles,  children = (elements {
            if items |> Seq.isEmpty then
                match whenEmpty with
                | Message message ->
                    LC.InfoMessage (level = InfoMessage.Level.Info, message = message)
                | Children children ->
                    children
            else
                match style with
                | Raw ->
                    whenNonempty items
                    seeAll |> Option.map (fun seeAll -> Helpers.SeeAll (seeAll, theTheme.SeeAll))
                | Responsive align ->
                    let alignmentStyle =
                        match align with
                        | HorizontalAlignment.Center -> Styles.AlignCenter
                        | HorizontalAlignment.Right  -> Styles.AlignRight
                        | HorizontalAlignment.Left   -> Styles.AlignLeft

                    RX.View (styles = [|Styles.ItemsBlock; alignmentStyle|], children = (elements {
                        whenNonempty items
                        seeAll |> Option.map (fun seeAll -> Helpers.SeeAll (seeAll, theTheme.SeeAll))
                    }))
                | Horizontal ->
                    RX.ScrollView (styles = [|Styles.ScrollView|], horizontal = true, children = elements {
                        RX.View (styles = [|Styles.Reel|], children = (elements {
                            RX.View (styles = [|Styles.Reel|], children = (elements {
                                whenNonempty items
                                seeAll |> Option.map (fun seeAll -> Helpers.SeeAll (seeAll, theTheme.SeeAll))
                            }))
                        }))
                    })

        }))
    }

and private Styles() =
    static member val ItemsBlock = makeViewStyles {
        FlexWrap.Wrap
        FlexDirection.Row
        paddingHorizontal 16
    }
    static member val AlignCenter = makeViewStyles { JustifyContent.Center    }
    static member val AlignRight  = makeViewStyles { JustifyContent.FlexEnd   }
    static member val AlignLeft   = makeViewStyles { JustifyContent.FlexStart }
    static member val ScrollView = makeScrollViewStyles {
        flex 0
    }
    static member val Reel = makeViewStyles {
        Overflow.Visible
        FlexDirection.Row
    }