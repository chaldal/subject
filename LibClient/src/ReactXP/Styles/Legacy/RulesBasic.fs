namespace ReactXP.LegacyStyles

open ReactXP.Styles.Animation
open ReactXP.LegacyStyles
open LibClient.ColorModule

open Fable.Core
open Fable.Core.JsInterop

module RulesRestricted =
    // Container layout
    [<StringEnum>] type AlignSelf = Auto | [<CompiledName("flex-start")>]FlexStart | [<CompiledName("flex-end")>]FlexEnd | Center | Stretch

    [<Emit("['alignSelf', $0]")>]
    let helper_alignSelf (_value: AlignSelf) : RawReactXPStyleRule = jsNative
    let alignSelf = fun v -> RuleFunctionReturnedStyleRules.One (helper_alignSelf v, StyleRuleType.Flexbox)

    // Child layout

    [<RequireQualifiedAccess>] [<StringEnum>] type AlignContent = Auto | [<CompiledName("flex-start")>]FlexStart | [<CompiledName("flex-end")>]FlexEnd | Center | Stretch
    [<RequireQualifiedAccess>] [<StringEnum>] type AlignItems = Stretch | [<CompiledName("flex-start")>]FlexStart | [<CompiledName("flex-end")>]FlexEnd | Center | Baseline
    [<RequireQualifiedAccess>] [<StringEnum>] type FlexWrap = Wrap | Nowrap
    [<RequireQualifiedAccess>] [<StringEnum>] type FlexDirection = Column | Row | [<CompiledName("column-reverse")>]ColumnReverse | [<CompiledName("row-reverse")>]RowReverse
    [<RequireQualifiedAccess>] [<StringEnum>] type JustifyContent = Center | [<CompiledName("flex-start")>]FlexStart | [<CompiledName("flex-end")>]FlexEnd | [<CompiledName("space-between")>]SpaceBetween | [<CompiledName("space-around")>]SpaceAround

    [<Emit("['alignContent', $0]")>]
    let helper_alignContent (_value: AlignContent) : RawReactXPStyleRule = jsNative
    let alignContent = fun v -> RuleFunctionReturnedStyleRules.One (helper_alignContent v, StyleRuleType.Flexbox)
    [<Emit("['alignItems', $0]")>]
    let helper_alignItems (_value: AlignItems) : RawReactXPStyleRule = jsNative
    let alignItems = fun v -> RuleFunctionReturnedStyleRules.One (helper_alignItems v, StyleRuleType.Flexbox)
    [<Emit("['flexWrap', $0]")>]
    let helper_flexWrap (_value: FlexWrap) : RawReactXPStyleRule = jsNative
    let flexWrap = fun v -> RuleFunctionReturnedStyleRules.One (helper_flexWrap v, StyleRuleType.Flexbox)
    [<Emit("['flexDirection', $0]")>]
    let helper_flexDirection (_value: FlexDirection) : RawReactXPStyleRule = jsNative
    let flexDirection = fun v -> RuleFunctionReturnedStyleRules.One (helper_flexDirection v, StyleRuleType.Flexbox)
    [<Emit("['justifyContent', $0]")>]
    let helper_justifyContent (_value: JustifyContent) : RawReactXPStyleRule = jsNative
    let justifyContent = fun v -> RuleFunctionReturnedStyleRules.One (helper_justifyContent v, StyleRuleType.Flexbox)

    // Position Overrides

    [<StringEnum>] type Position = Absolute | Relative

    [<Emit("['position', $0]")>]
    let helper_position (_value: Position) : RawReactXPStyleRule = jsNative
    let position = fun v -> RuleFunctionReturnedStyleRules.One (helper_position v, StyleRuleType.Flexbox)

    // Overflow

    [<StringEnum>]
    type Overflow = Hidden | Visible

    [<Emit("['overflow', $0]")>]
    let helper_overflow (_value: Overflow) : RawReactXPStyleRule = jsNative
    let overflow = fun v -> RuleFunctionReturnedStyleRules.One (helper_overflow v, StyleRuleType.View)

    // Text alignment
    [<RequireQualifiedAccess>]
    [<StringEnum>]
    type TextAlign = | Auto | Left | Right | Center | Justify

    [<RequireQualifiedAccess>]
    [<StringEnum>]
    type TextAlignVertical = | Auto | Top | Bottom | Center

    [<Emit("['textAlign', $0]")>]
    let helper_textAlign (_value: TextAlign) : RawReactXPStyleRule = jsNative
    let textAlign = fun v -> RuleFunctionReturnedStyleRules.One (helper_textAlign v, StyleRuleType.Text)
    [<Emit("['textAlignVertical', $0]")>]
    let helper_textAlignVertical (_value: TextAlignVertical) : RawReactXPStyleRule = jsNative
    let textAlignVertical = fun v -> RuleFunctionReturnedStyleRules.One (helper_textAlignVertical v, StyleRuleType.Text)

    // Text Style Attributes
    // Font Information

    // Attributes in this group cascade to child RX.Text components

    [<StringEnum>]
    type FontStyle = Normal | Italic
    [<StringEnum>]
    type FontWeight = Normal | Bold | [<CompiledName("100")>]W100 | [<CompiledName("200")>]W200 | [<CompiledName("300")>]W300 | [<CompiledName("400")>]W400 | [<CompiledName("500")>]W500 | [<CompiledName("600")>]W600 | [<CompiledName("700")>]W700 | [<CompiledName("800")>]W800 | [<CompiledName("900")>]W900


    [<Emit("['fontStyle', $0]")>]
    let helper_fontStyle (_value: FontStyle) : RawReactXPStyleRule = jsNative
    let fontStyle = fun v -> RuleFunctionReturnedStyleRules.One (helper_fontStyle v, StyleRuleType.Text)
    [<Emit("['fontWeight', $0]")>]
    let helper_fontWeight (_value: FontWeight) : RawReactXPStyleRule = jsNative
    let fontWeight = fun v -> RuleFunctionReturnedStyleRules.One (helper_fontWeight v, StyleRuleType.Text)

    [<StringEnum>]
    type Cursor = Pointer | Default // Web only

    [<Emit("['cursor', $0]")>]
    let helper_cursor (_value: Cursor) : RawReactXPStyleRule = jsNative
    let cursor = fun v -> RuleFunctionReturnedStyleRules.One (helper_cursor v, StyleRuleType.View)


[<AutoOpen>]
module RulesBasic =
    // Container layout
    [<Emit("['flex', $0]")>]
    let helper_flex (_value: int) : RawReactXPStyleRule = jsNative
    let flex = fun v -> RuleFunctionReturnedStyleRules.One (helper_flex v, StyleRuleType.View)

    [<Emit("['flexGrow', $0]")>]
    let helper_flexGrow (_value: int) : RawReactXPStyleRule = jsNative
    let flexGrow = fun v -> RuleFunctionReturnedStyleRules.One (helper_flexGrow v, StyleRuleType.View)

    [<Emit("['flexShrink', $0]")>]
    let helper_flexShrink (_value: int) : RawReactXPStyleRule = jsNative
    let flexShrink = fun v -> RuleFunctionReturnedStyleRules.One (helper_flexShrink v, StyleRuleType.View)

    [<Emit("['flexBasis', $0]")>]
    let helper_flexBasis (_value: int) : RawReactXPStyleRule = jsNative
    let flexBasis = fun v -> RuleFunctionReturnedStyleRules.One (helper_flexBasis v, StyleRuleType.View)

    type AlignSelf =
        static member Auto      = RulesRestricted.alignSelf RulesRestricted.AlignSelf.Auto
        static member FlexStart = RulesRestricted.alignSelf RulesRestricted.AlignSelf.FlexStart
        static member FlexEnd   = RulesRestricted.alignSelf RulesRestricted.AlignSelf.FlexEnd
        static member Center    = RulesRestricted.alignSelf RulesRestricted.AlignSelf.Center
        static member Stretch   = RulesRestricted.alignSelf RulesRestricted.AlignSelf.Stretch

    // Child layout

    type AlignContent =
        static member Auto      = RulesRestricted.alignContent RulesRestricted.AlignContent.Auto
        static member FlexStart = RulesRestricted.alignContent RulesRestricted.AlignContent.FlexStart
        static member FlexEnd   = RulesRestricted.alignContent RulesRestricted.AlignContent.FlexEnd
        static member Center    = RulesRestricted.alignContent RulesRestricted.AlignContent.Center
        static member Stretch   = RulesRestricted.alignContent RulesRestricted.AlignContent.Stretch

    type AlignItems =
        static member Stretch   = RulesRestricted.alignItems RulesRestricted.AlignItems.Stretch
        static member FlexStart = RulesRestricted.alignItems RulesRestricted.AlignItems.FlexStart
        static member FlexEnd   = RulesRestricted.alignItems RulesRestricted.AlignItems.FlexEnd
        static member Center    = RulesRestricted.alignItems RulesRestricted.AlignItems.Center
        static member Baseline  = RulesRestricted.alignItems RulesRestricted.AlignItems.Baseline

    type FlexWrap =
        static member Wrap   = RulesRestricted.flexWrap RulesRestricted.FlexWrap.Wrap
        static member Nowrap = RulesRestricted.flexWrap RulesRestricted.FlexWrap.Nowrap

    type FlexDirection =
        static member Column        = RulesRestricted.flexDirection RulesRestricted.FlexDirection.Column
        static member Row           = RulesRestricted.flexDirection RulesRestricted.FlexDirection.Row
        static member ColumnReverse = RulesRestricted.flexDirection RulesRestricted.FlexDirection.ColumnReverse
        static member RowReverse    = RulesRestricted.flexDirection RulesRestricted.FlexDirection.RowReverse

        static member ColumnReverseZindexHack = FlexDirection.ColumnReverse
        static member RowReverseZindexHack    = FlexDirection.RowReverse

    type JustifyContent =
        static member Center       = RulesRestricted.justifyContent RulesRestricted.JustifyContent.Center
        static member FlexStart    = RulesRestricted.justifyContent RulesRestricted.JustifyContent.FlexStart
        static member FlexEnd      = RulesRestricted.justifyContent RulesRestricted.JustifyContent.FlexEnd
        static member SpaceBetween = RulesRestricted.justifyContent RulesRestricted.JustifyContent.SpaceBetween
        static member SpaceAround  = RulesRestricted.justifyContent RulesRestricted.JustifyContent.SpaceAround

    // Size Overrides

    [<Emit("['height', $0]")>]
    let helper_height (_value: int) : RawReactXPStyleRule = jsNative
    let height = fun v -> RuleFunctionReturnedStyleRules.One (helper_height v, StyleRuleType.Flexbox)
    [<Emit("['width', $0]")>]
    let helper_width (_value: int) : RawReactXPStyleRule = jsNative
    let width = fun v -> RuleFunctionReturnedStyleRules.One (helper_width v, StyleRuleType.Flexbox)
    [<Emit("['maxHeight', $0]")>]
    let helper_maxHeight (_value: int) : RawReactXPStyleRule = jsNative
    let maxHeight = fun v -> RuleFunctionReturnedStyleRules.One (helper_maxHeight v, StyleRuleType.Flexbox)
    [<Emit("['maxWidth', $0]")>]
    let helper_maxWidth (_value: int) : RawReactXPStyleRule = jsNative
    let maxWidth = fun v -> RuleFunctionReturnedStyleRules.One (helper_maxWidth v, StyleRuleType.Flexbox)
    [<Emit("['minHeight', $0]")>]
    let helper_minHeight (_value: int) : RawReactXPStyleRule = jsNative
    let minHeight = fun v -> RuleFunctionReturnedStyleRules.One (helper_minHeight v, StyleRuleType.Flexbox)
    [<Emit("['minWidth', $0]")>]
    let helper_minWidth (_value: int) : RawReactXPStyleRule = jsNative
    let minWidth = fun v -> RuleFunctionReturnedStyleRules.One (helper_minWidth v, StyleRuleType.Flexbox)

    // Position Overrides

    type Position =
        static member Relative = RulesRestricted.position RulesRestricted.Position.Relative
        static member Absolute = RulesRestricted.position RulesRestricted.Position.Absolute

    [<Emit("['top', $0]")>]
    let helper_top (_value: int) : RawReactXPStyleRule = jsNative
    let top = fun v -> RuleFunctionReturnedStyleRules.One (helper_top v, StyleRuleType.Flexbox)
    [<Emit("['right', $0]")>]
    let helper_right (_value: int) : RawReactXPStyleRule = jsNative
    let right = fun v -> RuleFunctionReturnedStyleRules.One (helper_right v, StyleRuleType.Flexbox)
    [<Emit("['bottom', $0]")>]
    let helper_bottom (_value: int) : RawReactXPStyleRule = jsNative
    let bottom = fun v -> RuleFunctionReturnedStyleRules.One (helper_bottom v, StyleRuleType.Flexbox)
    [<Emit("['left', $0]")>]
    let helper_left (_value: int) : RawReactXPStyleRule = jsNative
    let left = fun v -> RuleFunctionReturnedStyleRules.One (helper_left v, StyleRuleType.Flexbox)

    // Margins

    [<Emit("['margin', $0]")>]
    let helper_margin (_value: int) : RawReactXPStyleRule = jsNative
    let margin = fun v -> RuleFunctionReturnedStyleRules.One (helper_margin v, StyleRuleType.Flexbox)
    [<Emit("['marginHorizontal', $0]")>]
    let helper_marginHorizontal (_value: int) : RawReactXPStyleRule = jsNative
    let marginHorizontal = fun v -> RuleFunctionReturnedStyleRules.One (helper_marginHorizontal v, StyleRuleType.Flexbox)
    [<Emit("['marginVertical', $0]")>]
    let helper_marginVertical (_value: int) : RawReactXPStyleRule = jsNative
    let marginVertical = fun v -> RuleFunctionReturnedStyleRules.One (helper_marginVertical v, StyleRuleType.Flexbox)
    [<Emit("['marginTop', $0]")>]
    let helper_marginTop (_value: int) : RawReactXPStyleRule = jsNative
    let marginTop = fun v -> RuleFunctionReturnedStyleRules.One (helper_marginTop v, StyleRuleType.Flexbox)
    [<Emit("['marginRight', $0]")>]
    let helper_marginRight (_value: int) : RawReactXPStyleRule = jsNative
    let marginRight = fun v -> RuleFunctionReturnedStyleRules.One (helper_marginRight v, StyleRuleType.Flexbox)
    [<Emit("['marginBottom', $0]")>]
    let helper_marginBottom (_value: int) : RawReactXPStyleRule = jsNative
    let marginBottom = fun v -> RuleFunctionReturnedStyleRules.One (helper_marginBottom v, StyleRuleType.Flexbox)
    [<Emit("['marginLeft', $0]")>]
    let helper_marginLeft (_value: int) : RawReactXPStyleRule = jsNative
    let marginLeft = fun v -> RuleFunctionReturnedStyleRules.One (helper_marginLeft v, StyleRuleType.Flexbox)

    // Gaps

    [<Emit("['gap', $0]")>]
    let helper_gap (_value: int) : RawReactXPStyleRule = jsNative
    let gap = fun v -> RuleFunctionReturnedStyleRules.One (helper_gap v, StyleRuleType.Flexbox)
    [<Emit("['rowGap', $0]")>]
    let helper_rowGap (_value: int) : RawReactXPStyleRule = jsNative
    let rowGap = fun v -> RuleFunctionReturnedStyleRules.One (helper_rowGap v, StyleRuleType.Flexbox)
    [<Emit("['columnGap', $0]")>]
    let helper_columnGap (_value: int) : RawReactXPStyleRule = jsNative
    let columnGap = fun v -> RuleFunctionReturnedStyleRules.One (helper_columnGap v, StyleRuleType.Flexbox)

    // Padding

    [<Emit("['padding', $0]")>]
    let helper_padding (_value: int) : RawReactXPStyleRule = jsNative
    let padding = fun v -> RuleFunctionReturnedStyleRules.One (helper_padding v, StyleRuleType.Flexbox)
    [<Emit("['paddingHorizontal', $0]")>]
    let helper_paddingHorizontal (_value: int) : RawReactXPStyleRule = jsNative
    let paddingHorizontal = fun v -> RuleFunctionReturnedStyleRules.One (helper_paddingHorizontal v, StyleRuleType.Flexbox)
    [<Emit("['paddingVertical', $0]")>]
    let helper_paddingVertical (_value: int) : RawReactXPStyleRule = jsNative
    let paddingVertical = fun v -> RuleFunctionReturnedStyleRules.One (helper_paddingVertical v, StyleRuleType.Flexbox)
    [<Emit("['paddingTop', $0]")>]
    let helper_paddingTop (_value: int) : RawReactXPStyleRule = jsNative
    let paddingTop = fun v -> RuleFunctionReturnedStyleRules.One (helper_paddingTop v, StyleRuleType.Flexbox)
    [<Emit("['paddingRight', $0]")>]
    let helper_paddingRight (_value: int) : RawReactXPStyleRule = jsNative
    let paddingRight = fun v -> RuleFunctionReturnedStyleRules.One (helper_paddingRight v, StyleRuleType.Flexbox)
    [<Emit("['paddingBottom', $0]")>]
    let helper_paddingBottom (_value: int) : RawReactXPStyleRule = jsNative
    let paddingBottom = fun v -> RuleFunctionReturnedStyleRules.One (helper_paddingBottom v, StyleRuleType.Flexbox)
    [<Emit("['paddingLeft', $0]")>]
    let helper_paddingLeft (_value: int) : RawReactXPStyleRule = jsNative
    let paddingLeft = fun v -> RuleFunctionReturnedStyleRules.One (helper_paddingLeft v, StyleRuleType.Flexbox)


    // View Style Attributes

    // Color & Opacity

    [<StringEnum>]
    type AcrylicSourceUWP = Host | App

    [<Emit("['backgroundColor', $0]")>]
    let private helper_backgroundColorString (_value: string) : RawReactXPStyleRule = jsNative
    let private backgroundColorString = fun v -> RuleFunctionReturnedStyleRules.One (helper_backgroundColorString v, StyleRuleType.View)
    let backgroundColor (value: Color) : RuleFunctionReturnedStyleRules =
        backgroundColorString value.ToReactXPString

    [<Emit("['opacity', $0]")>]
    let helper_opacity (_value: float) : RawReactXPStyleRule = jsNative
    let opacity = fun v -> RuleFunctionReturnedStyleRules.One (helper_opacity v, StyleRuleType.View)
    [<Emit("['acrylicOpacityUWP', $0]")>]
    let helper_acrylicOpacityUWP (_value: float) : RawReactXPStyleRule = jsNative
    let acrylicOpacityUWP = fun v -> RuleFunctionReturnedStyleRules.One (helper_acrylicOpacityUWP v, StyleRuleType.View)
    [<Emit("['acrylicSourceUWP', $0]")>]
    let helper_acrylicSourceUWP (_value: AcrylicSourceUWP) : RawReactXPStyleRule = jsNative
    let acrylicSourceUWP = fun v -> RuleFunctionReturnedStyleRules.One (helper_acrylicSourceUWP v, StyleRuleType.View)
    [<Emit("['acrylicTintColorUWP', $0]")>]
    let helper_acrylicTintColorUWP (_value: string) : RawReactXPStyleRule = jsNative
    let acrylicTintColorUWP = fun v -> RuleFunctionReturnedStyleRules.One (helper_acrylicTintColorUWP v, StyleRuleType.View)

    // Overflow
    type Overflow =
        static member Hidden               = RulesRestricted.overflow RulesRestricted.Overflow.Hidden
        static member Visible              = RulesRestricted.overflow RulesRestricted.Overflow.Visible
        static member VisibleForTapCapture = RulesRestricted.overflow RulesRestricted.Overflow.Visible
        static member VisibleForScrolling  = RulesRestricted.overflow RulesRestricted.Overflow.Visible
        static member VisibleForDropShadow = RulesRestricted.overflow RulesRestricted.Overflow.Visible

    // Borders
    [<StringEnum>]
    type BorderStyle =
    | Solid | Dotted | Dashed
    // DO NOT CLOBBER Option.None it's really annoying to debug
    | [<CompiledName("none")>]Non

    [<Emit("['borderWidth', $0]")>]
    let helper_borderWidth (_value: int) : RawReactXPStyleRule = jsNative
    let borderWidth = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderWidth v, StyleRuleType.View)
    [<Emit("['borderTopWidth', $0]")>]
    let helper_borderTopWidth (_value: int) : RawReactXPStyleRule = jsNative
    let borderTopWidth = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderTopWidth v, StyleRuleType.View)
    [<Emit("['borderRightWidth', $0]")>]
    let helper_borderRightWidth (_value: int) : RawReactXPStyleRule = jsNative
    let borderRightWidth = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderRightWidth v, StyleRuleType.View)
    [<Emit("['borderBottomWidth', $0]")>]
    let helper_borderBottomWidth (_value: int) : RawReactXPStyleRule = jsNative
    let borderBottomWidth = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderBottomWidth v, StyleRuleType.View)
    [<Emit("['borderLeftWidth', $0]")>]
    let helper_borderLeftWidth (_value: int) : RawReactXPStyleRule = jsNative
    let borderLeftWidth = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderLeftWidth v, StyleRuleType.View)
    [<Emit("['borderStyle', $0]")>]
    let helper_borderStyle (_value: BorderStyle) : RawReactXPStyleRule = jsNative
    let borderStyle = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderStyle v, StyleRuleType.View)
    [<Emit("['borderRadius', $0]")>]
    let helper_borderRadius (_value: int) : RawReactXPStyleRule = jsNative
    let borderRadius = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderRadius v, StyleRuleType.View)
    [<Emit("['borderTopRightRadius', $0]")>]
    let helper_borderTopRightRadius (_value: int) : RawReactXPStyleRule = jsNative
    let borderTopRightRadius = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderTopRightRadius v, StyleRuleType.View)
    [<Emit("['borderBottomRightRadius', $0]")>]
    let helper_borderBottomRightRadius (_value: int) : RawReactXPStyleRule = jsNative
    let borderBottomRightRadius = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderBottomRightRadius v, StyleRuleType.View)
    [<Emit("['borderBottomLeftRadius', $0]")>]
    let helper_borderBottomLeftRadius (_value: int) : RawReactXPStyleRule = jsNative
    let borderBottomLeftRadius = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderBottomLeftRadius v, StyleRuleType.View)
    [<Emit("['borderTopLeftRadius', $0]")>]
    let helper_borderTopLeftRadius (_value: int) : RawReactXPStyleRule = jsNative
    let borderTopLeftRadius = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderTopLeftRadius v, StyleRuleType.View)


    [<Emit("['borderColor', $0]")>]
    let private helper_borderColorString (_value: string) : RawReactXPStyleRule = jsNative
    let private borderColorString = fun v -> RuleFunctionReturnedStyleRules.One (helper_borderColorString v, StyleRuleType.View)

    let borderColor (value: Color) : RuleFunctionReturnedStyleRules =
        borderColorString value.ToReactXPString

    // Shadows

    // NOTE: If applied to a Text element, these properties translate to text shadows,
    // not a box shadow.
    type ShadowOffset = {
        height: int
        width:  int
    }
    [<Emit("['shadowOffset', $0]")>]
    let helper_shadowOffset (_value: ShadowOffset) : RawReactXPStyleRule = jsNative
    let shadowOffset = fun v -> RuleFunctionReturnedStyleRules.One (helper_shadowOffset v, StyleRuleType.View)
    [<Emit("['shadowRadius', $0]")>]
    let helper_shadowRadius (_value: int) : RawReactXPStyleRule = jsNative
    let shadowRadius = fun v -> RuleFunctionReturnedStyleRules.One (helper_shadowRadius v, StyleRuleType.View)
    [<Emit("['shadowOpacity', $0]")>]
    let helper_shadowOpacity (_value: int) : RawReactXPStyleRule = jsNative
    let shadowOpacity = fun v -> RuleFunctionReturnedStyleRules.One (helper_shadowOpacity v, StyleRuleType.View)
    [<Emit("['shadowColor', $0]")>]
    let private helper_shadowColorString (_value: string) : RawReactXPStyleRule = jsNative
    let private shadowColorString = fun v -> RuleFunctionReturnedStyleRules.One (helper_shadowColorString v, StyleRuleType.View)

    let shadowColor (value: Color) : RuleFunctionReturnedStyleRules =
        shadowColorString value.ToReactXPString

    [<Emit("['elevation', $0]")>]
    let helper_elevation (_value: int) : RawReactXPStyleRule = jsNative
    let elevation = fun v -> RuleFunctionReturnedStyleRules.One (helper_elevation v, StyleRuleType.View)

    // Miscellaneous
    [<StringEnum>]
    type WordBreak =
    | [<CompiledName("break-all")>]BreakAll
    | [<CompiledName("break-word")>]BreakWord // Web only
    [<StringEnum>]
    type AppRegion = Drag | [<CompiledName("no-drag")>]NoDrag // Web only

    [<Emit("['wordBreak', $0]")>]
    let helper_wordBreak (_value: WordBreak) : RawReactXPStyleRule = jsNative
    let wordBreak = fun v -> RuleFunctionReturnedStyleRules.One (helper_wordBreak v, StyleRuleType.View)
    [<Emit("['appRegion', $0]")>]
    let helper_appRegion (_value: AppRegion) : RawReactXPStyleRule = jsNative
    let appRegion = fun v -> RuleFunctionReturnedStyleRules.One (helper_appRegion v, StyleRuleType.View)

    type Cursor =
        static member Pointer = RulesRestricted.cursor RulesRestricted.Cursor.Pointer
        static member Default = RulesRestricted.cursor RulesRestricted.Cursor.Default

    // Transform Style Attributes
    // Transforms

    type TransformValue =
    | HackingForNiceSyntaxAndReasonableTypeSafety

    // All transform values are animatable
    let perspective (value: string) : TransformValue = ("perspective", value) :> obj :?> TransformValue
    let rotate (value: string) : TransformValue = ("rotate", value) :> obj :?> TransformValue
    let rotateX (value: string) : TransformValue = ("rotateX", value) :> obj :?> TransformValue
    let rotateY (value: string) : TransformValue = ("rotateY", value) :> obj :?> TransformValue
    let rotateZ (value: string) : TransformValue = ("rotateZ", value) :> obj :?> TransformValue
    let scale (value: float) : TransformValue = ("scale", value) :> obj :?> TransformValue
    let scaleX (value: float) : TransformValue = ("scaleX", value) :> obj :?> TransformValue
    let scaleY (value: float) : TransformValue = ("scaleY", value) :> obj :?> TransformValue
    let translateX (value: int) : TransformValue = ("translateX", value) :> obj :?> TransformValue
    let translateY (value: int) : TransformValue = ("translateY", value) :> obj :?> TransformValue

    [<Emit("['transform', $0]")>]
    let helper__transformHelper (_value: array<obj>) : RawReactXPStyleRule = jsNative
    let _transformHelper = fun v -> RuleFunctionReturnedStyleRules.One (helper__transformHelper v, StyleRuleType.Transform)

    let transform (value: List<List<TransformValue>>) : RuleFunctionReturnedStyleRules =
        value
        |> List.map (fun (v) -> createObj (v :> obj :?> List<string * obj>))
        |> Array.ofList
        |> _transformHelper

    // Animation rules

    [<Emit("['opacity', $0]")>]
    let helper_aniOpacity (_value: RawAnimatedValue) : RawReactXPStyleRule = jsNative
    let aniOpacity = fun v -> RuleFunctionReturnedStyleRules.One (helper_aniOpacity v, StyleRuleType.View)

    [<Emit("['height', $0]")>]
    let helper_aniHeight (_value: RawAnimatedValue) : RawReactXPStyleRule = jsNative
    let aniHeight = fun v -> RuleFunctionReturnedStyleRules.One (helper_aniHeight v, StyleRuleType.View)

    // Transform Style Attributes
    // Transforms

    type AniTransformValue =
    | AniHackingForNiceSyntaxAndReasonableTypeSafety

    // All transform values are animatable
    let aniPerspective (value: RawAnimatedValue) : AniTransformValue = ("perspective", value) :> obj :?> AniTransformValue
    let aniRotate (value: RawAnimatedValue) : AniTransformValue = ("rotate", value) :> obj :?> AniTransformValue
    let aniRotateX (value: RawAnimatedValue) : AniTransformValue = ("rotateX", value) :> obj :?> AniTransformValue
    let aniRotateY (value: RawAnimatedValue) : AniTransformValue = ("rotateY", value) :> obj :?> AniTransformValue
    let aniRotateZ (value: RawAnimatedValue) : AniTransformValue = ("rotateZ", value) :> obj :?> AniTransformValue
    let aniScale (value: RawAnimatedValue) : AniTransformValue = ("scale", value) :> obj :?> AniTransformValue
    let aniScaleX (value: RawAnimatedValue) : AniTransformValue = ("scaleX", value) :> obj :?> AniTransformValue
    let aniScaleY (value: RawAnimatedValue) : AniTransformValue = ("scaleY", value) :> obj :?> AniTransformValue
    let aniTranslateX (value: RawAnimatedValue) : AniTransformValue = ("translateX", value) :> obj :?> AniTransformValue
    let aniTranslateY (value: RawAnimatedValue) : AniTransformValue = ("translateY", value) :> obj :?> AniTransformValue

    let aniTransform (value: List<List<AniTransformValue>>) : RuleFunctionReturnedStyleRules =
        value
        |> List.map (fun (v) -> createObj (v :> obj :?> List<string * obj>))
        |> Array.ofList
        |> _transformHelper


    // Text Style Attributes
    // Font Information

    // Attributes in this group cascade to child RX.Text components
    [<Emit("['fontFamily', $0]")>]
    let helper_fontFamily (_value: string) : RawReactXPStyleRule = jsNative
    let fontFamily = fun v -> RuleFunctionReturnedStyleRules.One (helper_fontFamily v, StyleRuleType.View)
    [<Emit("['fontSize', $0]")>]
    let helper_fontSize (_value: int) : RawReactXPStyleRule = jsNative
    let fontSize = fun v -> RuleFunctionReturnedStyleRules.One (helper_fontSize v, StyleRuleType.View)

    type FontWeight =
        static member Normal = RulesRestricted.fontWeight RulesRestricted.FontWeight.Normal
        static member Bold   = RulesRestricted.fontWeight RulesRestricted.FontWeight.Bold
        static member W100   = RulesRestricted.fontWeight RulesRestricted.FontWeight.W100
        static member W200   = RulesRestricted.fontWeight RulesRestricted.FontWeight.W200
        static member W300   = RulesRestricted.fontWeight RulesRestricted.FontWeight.W300
        static member W400   = RulesRestricted.fontWeight RulesRestricted.FontWeight.W400
        static member W500   = RulesRestricted.fontWeight RulesRestricted.FontWeight.W500
        static member W600   = RulesRestricted.fontWeight RulesRestricted.FontWeight.W600
        static member W700   = RulesRestricted.fontWeight RulesRestricted.FontWeight.W700
        static member W800   = RulesRestricted.fontWeight RulesRestricted.FontWeight.W800
        static member W900   = RulesRestricted.fontWeight RulesRestricted.FontWeight.W900

    type FontStyle =
        static member Normal = RulesRestricted.fontStyle RulesRestricted.FontStyle.Normal
        static member Italic = RulesRestricted.fontStyle RulesRestricted.FontStyle.Italic


    // NOTE the `font` "shortcut" that's described in the docs is actually
    // a misdocumentation — the shortcut field names don't work, and instead
    // you have to use ones prefixed with `font`, which basically means it's
    // not a shortcut, just annoying nesting. So we're not implementing it.

    // Text Color

    // Attributes in this group cascade to child RX.Text components
    [<Emit("['color', $0]")>]
    let private helper_colorString (_value: string) : RawReactXPStyleRule = jsNative
    let private colorString = fun v -> RuleFunctionReturnedStyleRules.One (helper_colorString v, StyleRuleType.Text)
    let color (value: Color) : RuleFunctionReturnedStyleRules =
        colorString value.ToReactXPString

    // Spacing Overrides

    // Attributes in this group cascade to child RX.Text components
    [<Emit("['letterSpacing', $0]")>]
    let helper_letterSpacing (_value: int) : RawReactXPStyleRule = jsNative
    let letterSpacing = fun v -> RuleFunctionReturnedStyleRules.One (helper_letterSpacing v, StyleRuleType.Text)
    [<Emit("['lineHeight', $0]")>]
    let helper_lineHeight (_value: int) : RawReactXPStyleRule = jsNative
    let lineHeight = fun v -> RuleFunctionReturnedStyleRules.One (helper_lineHeight v, StyleRuleType.Text)

    // Alignment

    type TextAlign =
        static member Auto    = RulesRestricted.textAlign RulesRestricted.TextAlign.Auto
        static member Left    = RulesRestricted.textAlign RulesRestricted.TextAlign.Left
        static member Right   = RulesRestricted.textAlign RulesRestricted.TextAlign.Right
        static member Justify = RulesRestricted.textAlign RulesRestricted.TextAlign.Justify
        static member Center  = RulesRestricted.textAlign RulesRestricted.TextAlign.Center

    type TextAlignVertical =
        static member Auto    = RulesRestricted.textAlignVertical RulesRestricted.TextAlignVertical.Auto
        static member Top     = RulesRestricted.textAlignVertical RulesRestricted.TextAlignVertical.Top
        static member Bottom  = RulesRestricted.textAlignVertical RulesRestricted.TextAlignVertical.Bottom
        static member Center  = RulesRestricted.textAlignVertical RulesRestricted.TextAlignVertical.Center


    // Text Decoration

    [<StringEnum>]
    type TextDecorationLine =
    // DO NOT CLOBBER Option.None it's really annoying to debug
    | [<CompiledName("none")>]Non | Underline | [<CompiledName("line-through")>]LineThrough | [<CompiledName("underline line-through")>]UnderlineLineThrough
    [<StringEnum>]
    type TextDecorationStyle = Solid | Double | Dotted | Dashed

    [<Emit("['textDecorationLine', $0]")>]
    let helper_textDecorationLine (_value: TextDecorationLine) : RawReactXPStyleRule = jsNative
    let textDecorationLine = fun v -> RuleFunctionReturnedStyleRules.One (helper_textDecorationLine v, StyleRuleType.Text)
    [<Emit("['textDecorationStyle', $0]")>]
    let helper_textDecorationStyle (_value: TextDecorationStyle) : RawReactXPStyleRule = jsNative
    let textDecorationStyle = fun v -> RuleFunctionReturnedStyleRules.One (helper_textDecorationStyle v, StyleRuleType.Text)
    [<Emit("['textDecorationColor', $0]")>]
    let helper_textDecorationColor (_value: string) : RawReactXPStyleRule = jsNative
    let textDecorationColor = fun v -> RuleFunctionReturnedStyleRules.One (helper_textDecorationColor v, StyleRuleType.Text)

    // Writing Direction

    [<StringEnum>]
    type WritingDirection = Auto | Ltr | Rtl

    [<Emit("['writingDirection', $0]")>]
    let helper_writingDirection (_value: WritingDirection) : RawReactXPStyleRule = jsNative
    let writingDirection = fun v -> RuleFunctionReturnedStyleRules.One (helper_writingDirection v, StyleRuleType.Text)

    // Miscellaneous

    [<Emit("['includeFontPadding', $0]")>]
    let helper_includeFontPadding (_value: bool) : RawReactXPStyleRule = jsNative
    let includeFontPadding = fun v -> RuleFunctionReturnedStyleRules.One (helper_includeFontPadding v, StyleRuleType.Text)

