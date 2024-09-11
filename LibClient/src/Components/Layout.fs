[<AutoOpen>]
module LibClient.Components.Layout

open Fable.React
open LibClient
open ReactXP.Components
open ReactXP.Styles

module LC =
    [<RequireQualifiedAccess>]
    type CrossAxisAlignment =
    | FlexStart
    | FlexEnd
    | Center
    | Stretch

open LC

module private Styles =
    let gap = ViewStyles.Memoize (fun (value: int) -> makeViewStyles {
        gap value
    })

    let crossAxisAlignment = ViewStyles.Memoize (fun (value: CrossAxisAlignment) -> makeViewStyles {
        match value with
        | CrossAxisAlignment.FlexStart -> AlignItems.FlexStart
        | CrossAxisAlignment.FlexEnd   -> AlignItems.FlexEnd
        | CrossAxisAlignment.Center    -> AlignItems.Center
        | CrossAxisAlignment.Stretch   -> AlignItems.Stretch
    })

    let centered = makeViewStyles {
        FlexDirection.Row
        AlignItems.Center
        JustifyContent.Center
    }

    let constrained = ViewStyles.Memoize (fun (maybeMaxWidth: Option<int>) (maybeMinWidth: Option<int>) (maybeMaxHeight: Option<int>) (maybeMinHeight: Option<int>) -> makeViewStyles {
        match maybeMaxWidth  with Some value -> maxWidth  value | None -> ()
        match maybeMinWidth  with Some value -> minWidth  value | None -> ()
        match maybeMaxHeight with Some value -> maxHeight value | None -> ()
        match maybeMinHeight with Some value -> minHeight value | None -> ()
        flexGrow 1
    })

    let sized = ViewStyles.Memoize (fun (maybeWidth: Option<int>) (maybeHeight: Option<int>) -> makeViewStyles {
        match maybeWidth  with Some value -> width  value | None -> ()
        match maybeHeight with Some value -> height value | None -> ()
        flex 0
    })

    let shrink = makeViewStyles {
        flex 0
        AlignSelf.FlexStart
    }

    let row = makeViewStyles {
        FlexDirection.Row
    }

    let column = makeViewStyles {
        FlexDirection.Column
    }

    let appTopLevelLayoutResponsiveContainer = makeViewStyles {
        Position.Absolute
        trbl 0 0 0 0
    }

type LibClient.Components.Constructors.LC with
    /// <summary>Wrap the child in a constrained box</summary>
    /// <param name="child" type="ReactElement"/>
    /// <param name="maxWidth" type="int" default="None"/>
    /// <param name="minWidth" type="int" default="None"/>
    /// <param name="maxHeight" type="int" default="None"/>
    /// <param name="minHeight" type="int" default="None"/>
    ///
    /// <example>
    /// maxWidth
    /// <code>
    ///     LC.Constrained (
    ///         maxWidth = 200,
    ///         child = RX.View (
    ///             styles = [|Styles.greyExpandingBox|],
    ///             children = [|LC.Text lipsum|]
    ///         )
    ///     )
    /// </code>
    /// </example>
    ///
    /// <example>
    /// maxHeight
    /// <code>
    ///     LC.Constrained (
    ///         maxHeight = 100,
    ///         child = RX.View (
    ///             styles = [|Styles.greyExpandingBox|],
    ///             children = [|LC.Text lipsum|]
    ///         )
    ///     )
    /// </code>
    /// </example>
    ///
    /// <example>
    /// minWidth
    /// <code>
    ///     // hard to demonstrate minWidth without LC.Shrink wrapping the whole thing
    ///     LC.Shrink (
    ///         LC.Constrained (
    ///             minWidth = 150,
    ///             child = RX.View (
    ///                 styles = [|Styles.greyExpandingBox|],
    ///                 children = [|LC.Text "Little text"|]
    ///             )
    ///         )
    ///     )
    /// </code>
    /// </example>
    ///
    /// <example>
    /// minHeight
    /// <code>
    ///     // hard to demonstrate minHeight without LC.Shrink wrapping the whole thing
    ///     LC.Shrink (
    ///         LC.Constrained (
    ///             minHeight = 150,
    ///             child = RX.View (
    ///                 styles = [|Styles.greyExpandingBox|],
    ///                 children = [|LC.Text "Little text"|]
    ///             )
    ///         )
    ///     )
    /// </code>
    /// </example>
    ///
    /// <remarks>
    ///     Setup code
    ///     <code setup="true">
    ///         module private Styles =
    ///             let greyExpandingBox = makeViewStyles {
    ///                 backgroundColor Color.DevLightGrey
    ///                 flex 1
    ///             }
    ///
    ///         let private lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In hendrerit vehicula sollicitudin. Sed lacinia, libero ultrices mattis dignissim, libero purus interdum ante, eu pharetra tortor massa vel lorem. Donec dignissim felis quis nisl sodales, id lacinia felis congue. Sed porta ipsum sem, et interdum arcu fringilla ac. Maecenas tincidunt, leo non ultricies molestie, lectus odio sagittis tellus, a convallis neque sapien vel sem. Nullam a justo blandit, condimentum leo sit amet, egestas lectus. Nunc eu eros eget lorem condimentum facilisis eget ac leo. Nulla bibendum ex eu dui blandit, sit amet feugiat quam interdum. Maecenas iaculis pharetra ex a gravida. Integer faucibus venenatis commodo."
    ///     </code>
    /// </remarks>
    [<Component>]
    static member Constrained (child: ReactElement, ?maxWidth: int, ?minWidth: int, ?maxHeight: int, ?minHeight: int) : ReactElement =
        RX.View (
            styles   = [|Styles.constrained maxWidth minWidth maxHeight minHeight|],
            children = [|child|]
        )

    [<Component>]
    static member Shrink (child: ReactElement, ?styles: array<ViewStyles>) : ReactElement =
        RX.View (
            styles =
                (Array.append
                    (defaultArg styles [||])
                    [|Styles.shrink|]
                ),
            children = [|child|]
        )

    /// <summary>Wrap the child in a fixed size box</summary>
    /// <param name="child" type="ReactElement"/>
    /// <param name="width" type="int" default="None"/>
    /// <param name="height" type="int" default="None"/>
    /// <param name="styles" type="array&lt;ViewStyles&gt;" default="[||]"/>
    /// <example>
    /// Basics
    /// <code>
    ///     LC.Sized (
    ///         width = 100,
    ///         height = 100,
    ///         child = RX.View (
    ///             styles = [|Styles.greyExpandingBox|],
    ///             children = [|LC.Text "the box"|]
    ///         )
    ///     )
    /// </code>
    /// </example>
    ///
    /// <example>
    /// Only width
    /// <code>
    ///     LC.Sized (
    ///         width = 100,
    ///         child = RX.View (
    ///             styles = [|Styles.greyExpandingBox|],
    ///             children = [|LC.Text "the box"|]
    ///         )
    ///     )
    /// </code>
    /// </example>
    ///
    /// <example>
    /// Only height
    /// <code>
    ///     LC.Sized (
    ///         height = 100,
    ///         child = RX.View (
    ///             styles = [|Styles.greyExpandingBox|],
    ///             children = [|LC.Text "the box"|]
    ///         )
    ///     )
    /// </code>
    /// </example>
    ///
    /// <remarks>
    ///     Setup code
    ///     <code setup="true">
    ///         module private Styles =
    ///             let greyExpandingBox = makeViewStyles {
    ///                 backgroundColor Color.DevLightGrey
    ///                 flex 1
    ///             }
    ///     </code>
    /// </remarks>

    [<Component>]
    static member Sized (?child: ReactElement, ?width: int, ?height: int, ?styles: array<ViewStyles>) : ReactElement =
        RX.View (
            styles =
                (Array.append
                    (defaultArg styles [||])
                    [|Styles.sized width height|]
                ),
            children = match child with Some value -> [|value|] | None -> [||]
        )

    [<Component>]
    static member Centered (child: ReactElement) : ReactElement =
        RX.View (
            styles   = [|Styles.centered|],
            children = [|child|]
        )

    /// <summary>Lay out the children in a row, optionally configuring the gap between children, and the vertical alignment.</summary>
    /// <param name="children" type="array&lt;ReactElement&gt;"/>
    /// <param name="crossAxisAlignment" type="CrossAxisAlignment" default="CrossAxisAlignment.Stretch">Alignment of children along the vertical axis</param>
    /// <param name="gap" type="int" default="0">Vertical gap between children</param>
    /// <param name="styles" type="array&lt;ViewStyles&gt;" default="[||]"/>
    ///
    /// <example>
    /// Basics
    /// <code>
    ///     LC.Row [|
    ///         LC.Text "Banana"
    ///         LC.Text "Apple"
    ///         LC.Text "Mango"
    ///     |]
    /// </code>
    /// </example>

    /// <example>
    /// Gap
    /// <code>
    ///     LC.Row (
    ///         gap = 10,
    ///         children = [|
    ///             LC.Text "Banana"
    ///             LC.Text "Apple"
    ///             LC.Text "Mango"
    ///         |]
    ///     )
    ///     LC.Row (
    ///         gap = 30,
    ///         children = [|
    ///             LC.Text "Banana"
    ///             LC.Text "Apple"
    ///             LC.Text "Mango"
    ///         |]
    ///     )
    /// </code>
    /// </example>
    ///
    /// <example>
    /// Cross axis alignment
    /// <code>
    ///     LC.Row (
    ///         crossAxisAlignment = LC.CrossAxisAlignment.Center,
    ///         children = [|
    ///             LC.Text "Banana"
    ///             LC.Text "Green\nApple"
    ///             LC.Text "Mango"
    ///         |]
    ///     )
    ///     LC.Row (
    ///         crossAxisAlignment = LC.CrossAxisAlignment.FlexStart,
    ///         children = [|
    ///             LC.Text "Banana"
    ///             LC.Text "Green\nApple"
    ///             LC.Text "Mango"
    ///         |]
    ///     )
    /// </code>
    /// </example>
    [<Component>]
    static member Row (children: array<ReactElement>, ?crossAxisAlignment: CrossAxisAlignment, ?gap: int, ?styles: array<ViewStyles>) : ReactElement =
        let theCrossAxisAlignment = defaultArg crossAxisAlignment CrossAxisAlignment.Center

        RX.View(
            styles =
                (Array.append
                    (defaultArg styles [||])
                    [|
                        Styles.row
                        Styles.crossAxisAlignment theCrossAxisAlignment
                        match gap with
                        | Some value -> Styles.gap value
                        | None -> ()
                    |]
                ),
            children = children
        )

    /// <summary>Lay out the children in a column, optionally configuring the gap beteween children, and the horizontal alignment</summary>
    /// <param name="children" type="array&lt;ReactElement&gt;"/>
    /// <param name="crossAxisAlignment" type="CrossAxisAlignment" default="CrossAxisAlignment.Stretch">Alignment of children along the horizontal axis</param>
    /// <param name="gap" type="int" default="0">Vertical gap between children</param>
    /// <param name="styles" type="array&lt;ViewStyles&gt;" default="[||]"/>
    ///
    /// <example>
    /// Basics
    /// <code>
    ///     LC.Column [|
    ///         LC.Text "Banana"
    ///         LC.Text "Apple"
    ///         LC.Text "Mango"
    ///     |]
    /// </code>
    /// </example>
    ///
    /// <example>
    /// Gap
    /// <code>
    ///     LC.Column (
    ///         gap = 30,
    ///         children = [|
    ///             LC.Text "Banana"
    ///             LC.Text "Apple"
    ///             LC.Text "Mango"
    ///         |]
    ///     )
    /// </code>
    /// </example>
    ///
    /// <example>
    /// Cross Axis Alignment
    /// <code>
    ///     LC.Column (
    ///         crossAxisAlignment = LC.CrossAxisAlignment.Center,
    ///         children = [|
    ///             LC.Text "Banana"
    ///             LC.Text "Green\nApple"
    ///             LC.Text "Mango"
    ///         |]
    ///     )
    ///     LC.Column (
    ///         crossAxisAlignment = LC.CrossAxisAlignment.FlexEnd,
    ///         children = [|
    ///             LC.Text "Banana"
    ///             LC.Text "Green\nApple"
    ///             LC.Text "Mango"
    ///         |]
    ///     )
    /// </code>
    /// </example>
    [<Component>]
    static member Column (children: array<ReactElement>, ?crossAxisAlignment: CrossAxisAlignment, ?gap: int, ?styles: array<ViewStyles>) : ReactElement =
        let theCrossAxisAlignment = defaultArg crossAxisAlignment CrossAxisAlignment.Stretch

        RX.View (
            styles =
                (Array.append
                    (defaultArg styles [||])
                    [|
                        Styles.column
                        Styles.crossAxisAlignment theCrossAxisAlignment
                        match gap with
                        | Some value -> Styles.gap value
                        | None -> ()
                    |]
                ),
            children = children
        )

    [<Component>]
    static member AppTopLevelLayoutResponsiveContainer (children: array<ReactElement>) : ReactElement =
        let forceUpdateReducer = Hooks.useReducer ((fun x _ -> x + 1), 0)

        // technically we should be disposing these, but because it's the top level app,
        // we don't care what happens when it goes away.
        Hooks.useEffect (fun () ->
            LibClient.Responsive.addOnScreenSizeUpdatedListener (System.Action (fun () -> forceUpdateReducer.update())) |> ignore
            ()
        )
        LibClient.Responsive.screenSizeContextProvider (LibClient.Responsive.getLatestScreenSize())
            [|
                RX.View (
                    onLayout = LibClient.Responsive.screenSizeOnLayout,
                    styles   = [|Styles.appTopLevelLayoutResponsiveContainer|],
                    children = children
                )
            |]
