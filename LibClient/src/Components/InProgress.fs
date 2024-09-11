[<AutoOpen>]
module LibClient.Components.InProgress

open Fable.React
open ReactXP.Styles
open ReactXP.Components
open LibClient

module private Styles =
    let view = makeViewStyles {
        FlexDirection.Column
    }

    let spinnerBlock = makeViewStyles {
        Position.Absolute
        trbl 0 0 0 0
        backgroundColor (Color.WhiteAlpha 0.5)
        FlexDirection.Row
        JustifyContent.Center
        AlignItems.Center
    }

type LC with
    /// <summary>Superimpose a spinner and a scrim on top of children when in progress. TODO: theme for scrim colour, spinner colour and size.</summary>
    /// <param name="isInProgress" type="bool"/>
    /// <param name="children" type="array&lt;ReactElement&gt;"/>
    /// <param name="styles" type="array&lt;ViewStyles&gt;" default="[||]"/>
    /// <example>
    /// Basics
    /// <code>
    ///     LC.Column (gap = 20, children = [|
    ///         LC.InProgress (false, [|
    ///             LC.InfoMessage "Some content here"
    ///         |])
    ///         LC.InProgress (true, [|
    ///             LC.InfoMessage "Some content here"
    ///         |])
    ///     |])
    /// </code>
    /// </example>

    [<Component>]
    static member InProgress (isInProgress: bool, children: array<ReactElement>, ?styles: array<ViewStyles>) : ReactElement =
        RX.View (
            styles =
                (Array.append
                    (defaultArg styles [||])
                    [|Styles.view|]
                ),
            children = elements {
                children
                if isInProgress then
                    RX.View (styles = [|Styles.spinnerBlock|], children = [|
                        RX.ActivityIndicator (
                            size = ActivityIndicator.Size.Tiny,
                            color = "#CCCCCC"
                        )
                    |])
            }
        )
