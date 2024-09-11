# Component Themeing

We have support for themeing F# dialect components, including setting
the default theme and partially overriding it. We also support legacy
theme overrides for legacy components.

## Using legacy themes

Here is how one uses a legacy theme inside an F# dialect component:

```fsharp
type Ui with
    [<Component>]
    static member Link (dispatchZoneId: DispatchZoneId, ?onFilter: (ReactEvent.Action -> DispatchZoneId -> unit)) : ReactElement =
        // redacted for brevity
        LC.IconButton(
            xLegacyStyles = Styles.IconFilter,
            icon = Icon.Funnel,
            state = (ButtonHighLevelState.LowLevel(Actionable (fun e -> onFilter e dispatchZoneId)))
        )

and private Styles =
    static member IconFilter =
        LibClient.Components.IconButtonStyles.Theme.One (LibClient.Input.ButtonLowLevelState.Actionable ignore, colors.Secondary.Main, 20, (0, 0, 0, 0))
        |> legacyTheme
```

The `legacyTheme` function is available from `open ReactXP.Styles`, and the
`xLegacyStyles` parameter is automatically generated on all RederDSL-based
legacy compoents.

## Creating themes for New F# Dialect Components

If you want your component to be themeable, in the component's file define the
`Theme` type,

```fsharp
module Ui =
    module FromTo =
        type Theme = {
            Color:    Color
            FontSize: int
        }
```

then open the module to make it available

```fsharp
open Ui.FromTo
```

then take a theme updater as an optional parameter

```fsharp
type Ui with
    [<Component>]
    static member FromTo (
        fromName: string,
        toName:   string,
        ?theme:   Theme -> Theme
    ) : ReactElement =
        LC.Text $"From {fromName} to {toName}"
```

Now obtain the updated (from the global setting) theme, and use it:

```fsharp
type Ui with
    [<Component>]
    static member FromTo (
        fromName: string,
        toName:   string,
        ?theme:   Theme -> Theme
    ) : ReactElement =
        let theTheme = Themes.GetMaybeUpdatedWith theme
        LC.Text ($"From {fromName} to {toName}", styles = [|theTheme.Text|])

and Ui.FromTo.Theme with
    member this.Text = makeTextStyles {
        color    this.Color
        fontSize this.FontSize
    }
```

You also need to provide the default theme, in your app's `ComponentsTheme.fs` file, or
the `DefaultComponentsTheme.fs` if the component in question is in a lib.

```fsharp
module AppExample.ComponentsTheme

open LibClient
open AppExample.Colors
open AppExample.Components

let applyTheme () : unit =
    // redacted for brevity

    Themes.Set<Ui.FromTo.Theme> { Color = Color.DevRed; FontSize = 12 }
```

Finally, at the use site where you want to use an updated theme:

```fsharp
Ui.FromTo (
    fromString,
    toString,
    theme = fun theme -> { theme with Color = Color.DevBlue }
)
```

### Warning
Be careful to not reuse theme types between components. Every themed component
needs to have a unique type representing its theme, because theme types are
the keys to their registration in the global theme, so if a theme is reused,
clobber occurs, and clobber can be difficult to debug.

## Using themes of F# components from within RenderDSL

This is fairly straightforward. Say you have an F# component `Foo`, and you want to
use it, themed, from inside `Sidebar` which is a legacy component. In `Sidebar.styles.fs` you'll
need to `open` the `Components` module where `Foo` lives, and then as a top level let-bound value,
declare your themed style:

```fsharp

module AppEggShellGallery.Components.SidebarStyles

open ReactXP.LegacyStyles
open AppEggShellGallery.Components

let styles = lazy (compile [
    // your regular stuff here
])

let foo (theme: Ui.Foo.Theme) = { theme with FontSize = 11 }
```

Then in `Sidebar.render`, you pass the theme to the component:

```xml
<Foo rt-fs='true' theme='SidebarStyles.foo'/>
```
