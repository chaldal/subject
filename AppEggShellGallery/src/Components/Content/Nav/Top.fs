[<AutoOpen>]
module AppEggShellGallery.Components.Content_Nav_Top

open Fable.React
open LibClient
open LibClient.LocalImages
open LibRouter.Components
open LibClient.Components
open LibClient.Responsive
open LibClient.Components.Layout.LC
open ReactXP.Styles

open AppEggShellGallery.Icons

module private Styles =
    let image = makeViewStyles {
        height       32
        width        32
        borderRadius 16
        marginRight  10
    }

    let heading = makeTextStyles {
        color Color.DevRed
    }

    let adjustIcon =
        LibClient.Components.Nav.Top.ItemStyles.Theme.IconVerticalPositionAdjustment 10
        |> legacyTheme

type Ui.Content.Nav with
    [<Component>]
    static member Test () : ReactElement =
        LC.Column [|
            LC.Text "banana"
            LC.Text "apple"
            LC.Text "mango"
        |]

    [<Component>]
    static member Top () : ReactElement =
        Ui.ComponentContent (
            displayName = "Nav.Top",
            isResponsive = true,
            props = ComponentContent.Manual (element {
                Ui.ScrapedComponentProps (heading = "Nav.Top.Base",              fullyQualifiedName = "LibClient.Components.Nav.Top.Base")
                Ui.ScrapedComponentProps (heading = "Nav.Top.Item",              fullyQualifiedName = "LibClient.Components.Nav.Top.Item")
                Ui.ScrapedComponentProps (heading = "Nav.Top.Heading",           fullyQualifiedName = "LibClient.Components.Nav.Top.Heading")
                Ui.ScrapedComponentProps (heading = "Nav.Top.Image",             fullyQualifiedName = "LibClient.Components.Nav.Top.Image")
                Ui.ScrapedComponentProps (heading = "Nav.Top.Filler",            fullyQualifiedName = "LibClient.Components.Nav.Top.Filler")
                Ui.ScrapedComponentProps (heading = "Nav.Top.ShowSidebarButton", fullyQualifiedName = "LibClient.Components.Nav.Top.ShowSidebarButton")
            }),
            samples = (element {
                Ui.ComponentSampleGroup (
                    samples = (element {
                        Ui.ComponentSample (
                            heading = "Basics",
                            layout = ComponentSample.Layout.CodeBelowSamples,
                            visuals = (
                                LC.With.Context (
                                    context = AppEggShellGallery.SampleVisualsScreenSize.sampleVisualsScreenSizeContext,
                                    ``with`` = (fun sampleVisualsScreenSize ->
                                        LC.ForceContext (context = screenSizeContext, value = sampleVisualsScreenSize, children = [|
                                            LC.Nav.Top.Base (
                                                desktop = fun _ -> element {
                                                    LC.Nav.Top.Image (source = localImage "/images/avatar.png", styles = [|Styles.image|])
                                                    LC.Nav.Top.Heading "MATERIAL DESIGN"
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Dummy Inprogress",   state = Nav.Top.Item.InProgress)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Design",             state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Components",         state = Nav.Top.Item.Selected)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Develop",            state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Blog",               state = Nav.Top.Item.Disabled)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.MagnifyingGlass, state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (
                                                        style = Nav.Top.Item.Style.With (label = "Cart", icon = Icon.ShoppingCart, badge = Nav.Top.Item.Count 3),
                                                        state = Nav.Top.Item.Actionable ignore
                                                    )
                                                },
                                                handheld = fun _ -> element {
                                                    LR.Nav.Top.BackButton()
                                                    LC.Nav.Top.Heading "MATERIAL DESIGN"
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Home",    state = Nav.Top.Item.Selected)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.Menu, state = Nav.Top.Item.Actionable ignore)
                                                }
                                            )
                                        |])
                                    )
                                )
                            ),
                            code = ComponentSample.SingleBlock (ComponentSample.Fsharp, LC.Text """
                                            LC.Nav.Top.Base (
                                                desktop = fun _ -> element {
                                                    LC.Nav.Top.Image (source = localImage "/images/avatar.png", styles = [|Styles.image|])
                                                    LC.Nav.Top.Heading "MATERIAL DESIGN"
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Dummy Inprogress",   state = Nav.Top.Item.InProgress)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Design",             state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Components",         state = Nav.Top.Item.Selected)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Develop",            state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Blog",               state = Nav.Top.Item.Disabled)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.MagnifyingGlass, state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (
                                                        style = Nav.Top.Item.Style.With (label = "Cart", icon = Icon.ShoppingCart, badge = Nav.Top.Item.Count 3),
                                                        state = Nav.Top.Item.Actionable ignore
                                                    )
                                                },
                                                handheld = fun _ -> element {
                                                    LR.Nav.Top.BackButton()
                                                    LC.Nav.Top.Heading "MATERIAL DESIGN"
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Home",    state = Nav.Top.Item.Selected)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.Menu, state = Nav.Top.Item.Actionable ignore)
                                                }
                                            )

                                            module private Styles =
                                                let image = makeViewStyles {
                                                    height       32
                                                    width        32
                                                    borderRadius 16
                                                    marginRight  10
                                                }
                            """)
                        )

                        Ui.ComponentSample (
                            heading = "Without Heading",
                            layout = ComponentSample.Layout.CodeBelowSamples,
                            visuals = (
                                LC.With.Context (
                                    context = AppEggShellGallery.SampleVisualsScreenSize.sampleVisualsScreenSizeContext,
                                    ``with`` = (fun sampleVisualsScreenSize ->
                                        LC.ForceContext (context = screenSizeContext, value = sampleVisualsScreenSize, children = [|
                                            LC.Nav.Top.Base (
                                                desktop = fun _ -> element {
                                                    LC.Nav.Top.Image (source = localImage "/images/avatar.png", styles = [|Styles.image|])
                                                    LC.Nav.Top.Filler ()
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Dummy Inprogress",   state = Nav.Top.Item.InProgress)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Design",             state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Components",         state = Nav.Top.Item.Selected)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Develop",            state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Blog",               state = Nav.Top.Item.Disabled)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.MagnifyingGlass, state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (
                                                        style = Nav.Top.Item.Style.With (label = "Cart", icon = Icon.ShoppingCart, badge = Nav.Top.Item.Count 3),
                                                        state = Nav.Top.Item.Actionable ignore
                                                    )
                                                },
                                                handheld = fun _ -> element {
                                                    LR.Nav.Top.BackButton()
                                                    LC.Nav.Top.Filler ()
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Home",    state = Nav.Top.Item.Selected)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.Menu, state = Nav.Top.Item.Actionable ignore)
                                                }
                                            )
                                        |])
                                    )
                                )
                            ),
                            code = ComponentSample.SingleBlock (ComponentSample.Fsharp, LC.Text """
                                            LC.Nav.Top.Base (
                                                desktop = fun _ -> element {
                                                    LC.Nav.Top.Image (source = localImage "/images/avatar.png", styles = [|Styles.image|])
                                                    LC.Nav.Top.Filler ()
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Dummy Inprogress",   state = Nav.Top.Item.InProgress)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Design",             state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Components",         state = Nav.Top.Item.Selected)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Develop",            state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Blog",               state = Nav.Top.Item.Disabled)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.MagnifyingGlass, state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (
                                                        style = Nav.Top.Item.Style.With (label = "Cart", icon = Icon.ShoppingCart, badge = Nav.Top.Item.Count 3),
                                                        state = Nav.Top.Item.Actionable ignore
                                                    )
                                                },
                                                handheld = fun _ -> element {
                                                    LR.Nav.Top.BackButton()
                                                    LC.Nav.Top.Filler ()
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Home",    state = Nav.Top.Item.Selected)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.Menu, state = Nav.Top.Item.Actionable ignore)
                                                }
                                            )

                                            module private Styles =
                                                let image = makeViewStyles {
                                                    height       32
                                                    width        32
                                                    borderRadius 16
                                                    marginRight  10
                                                }
                            """)
                        )

                        Ui.ComponentSample (
                            heading = "Style Sample",
                            layout = ComponentSample.Layout.CodeBelowSamples,
                            visuals = (
                                LC.With.Context (
                                    context = AppEggShellGallery.SampleVisualsScreenSize.sampleVisualsScreenSizeContext,
                                    ``with`` = (fun sampleVisualsScreenSize ->
                                        LC.ForceContext (context = screenSizeContext, value = sampleVisualsScreenSize, children = [|
                                            LC.Nav.Top.Base (
                                                desktop = fun _ -> element {
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(label = "Store", badge = Nav.Top.Item.Text "Summer Sale"),               state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(icon  = Icon.Bell, badge = Nav.Top.Item.Count 2),                        state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Blog",                                                                   state = Nav.Top.Item.Disabled)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.MagnifyingGlass,                                                     state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(label = "Cart", icon = Icon.ShoppingCart, badge = Nav.Top.Item.Count 3), state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly Icon.X,                                                                    state = Nav.Top.Item.Actionable ignore, xLegacyStyles = Styles.adjustIcon)
                                                    LC.Nav.Top.Heading ("Heading", styles = [|Styles.heading|])
                                                },
                                                handheld = fun _ -> element {
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(label = "Store", badge = Nav.Top.Item.Text "Summer Sale"),               state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(icon  = Icon.Bell, badge = Nav.Top.Item.Count 2),                        state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Blog",                                                                   state = Nav.Top.Item.Disabled)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.MagnifyingGlass,                                                     state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(label = "Cart", icon = Icon.ShoppingCart, badge = Nav.Top.Item.Count 3), state = Nav.Top.Item.Actionable ignore)
                                                }
                                            )
                                        |])
                                    )
                                )
                            ),
                            code = ComponentSample.SingleBlock (ComponentSample.Fsharp, LC.Text """
                                            LC.Nav.Top.Base (
                                                desktop = fun _ -> element {
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(label = "Store", badge = Nav.Top.Item.Text "Summer Sale"),               state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(icon  = Icon.Bell, badge = Nav.Top.Item.Count 2),                        state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Blog",                                                                   state = Nav.Top.Item.Disabled)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.MagnifyingGlass,                                                     state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(label = "Cart", icon = Icon.ShoppingCart, badge = Nav.Top.Item.Count 3), state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly Icon.X,                                                                    state = Nav.Top.Item.Actionable ignore, xLegacyStyles = Styles.adjustIcon)
                                                    LC.Nav.Top.Heading ("Heading", styles = [|Styles.heading|])
                                                },
                                                handheld = fun _ -> element {
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(label = "Store", badge = Nav.Top.Item.Text "Summer Sale"),               state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(icon  = Icon.Bell, badge = Nav.Top.Item.Count 2),                        state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.labelOnly "Blog",                                                                   state = Nav.Top.Item.Disabled)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.iconOnly  Icon.MagnifyingGlass,                                                     state = Nav.Top.Item.Actionable ignore)
                                                    LC.Nav.Top.Item (style = Nav.Top.Item.Style.With(label = "Cart", icon = Icon.ShoppingCart, badge = Nav.Top.Item.Count 3), state = Nav.Top.Item.Actionable ignore)
                                                }
                                            )

                                            module private Styles =
                                                let heading = makeTextStyles {
                                                    color Color.DevRed
                                                }

                                                let adjustIcon =
                                                    LibClient.Components.Nav.Top.ItemStyles.Theme.IconVerticalPositionAdjustment 10
                                                    |> legacyTheme
                            """)
                        )
                    })
                )
            })
        )
