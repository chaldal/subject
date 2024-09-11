namespace ReactXP.Styles

open ReactXP.Styles
open ReactXP.Styles.Animation
open LibClient.ColorModule

open Fable.Core
open Fable.Core.JsInterop

module RulesRestricted =
    // Container layout
    [<StringEnum>] type AlignSelf = Auto | [<CompiledName("flex-start")>]FlexStart | [<CompiledName("flex-end")>]FlexEnd | Center | Stretch

    [<Emit("['alignSelf', $0]")>]
    let helper_alignSelf (_value: AlignSelf) : RawReactXPFlexStyleRule = jsNative
    let alignSelf = fun v -> helper_alignSelf v

    // Child layout

    [<RequireQualifiedAccess>] [<StringEnum>] type AlignContent = Auto | [<CompiledName("flex-start")>]FlexStart | [<CompiledName("flex-end")>]FlexEnd | Center | Stretch
    [<RequireQualifiedAccess>] [<StringEnum>] type AlignItems = Stretch | [<CompiledName("flex-start")>]FlexStart | [<CompiledName("flex-end")>]FlexEnd | Center | Baseline
    [<RequireQualifiedAccess>] [<StringEnum>] type FlexWrap = Wrap | Nowrap
    [<RequireQualifiedAccess>] [<StringEnum>] type FlexDirection = Column | Row | [<CompiledName("column-reverse")>]ColumnReverse | [<CompiledName("row-reverse")>]RowReverse
    [<RequireQualifiedAccess>] [<StringEnum>] type JustifyContent = Center | [<CompiledName("flex-start")>]FlexStart | [<CompiledName("flex-end")>]FlexEnd | [<CompiledName("space-between")>]SpaceBetween | [<CompiledName("space-around")>]SpaceAround

    [<Emit("['alignContent', $0]")>]
    let helper_alignContent (_value: AlignContent) : RawReactXPFlexChildrenStyleRule = jsNative
    let alignContent = fun v -> helper_alignContent v
    [<Emit("['alignItems', $0]")>]
    let helper_alignItems (_value: AlignItems) : RawReactXPFlexChildrenStyleRule = jsNative
    let alignItems = fun v -> helper_alignItems v
    [<Emit("['flexWrap', $0]")>]
    let helper_flexWrap (_value: FlexWrap) : RawReactXPFlexChildrenStyleRule = jsNative
    let flexWrap = fun v -> helper_flexWrap v
    [<Emit("['flexDirection', $0]")>]
    let helper_flexDirection (_value: FlexDirection) : RawReactXPFlexStyleRule = jsNative
    let flexDirection = fun v -> helper_flexDirection v
    [<Emit("['justifyContent', $0]")>]
    let helper_justifyContent (_value: JustifyContent) : RawReactXPFlexChildrenStyleRule = jsNative
    let justifyContent = fun v -> helper_justifyContent v

    // Position Overrides

    [<StringEnum>] type Position = Absolute | Relative

    [<Emit("['position', $0]")>]
    let helper_position (_value: Position) : RawReactXPFlexStyleRule = jsNative
    let position = fun v -> helper_position v

    // Overflow

    [<StringEnum>]
    type Overflow = Hidden | Visible

    [<Emit("['overflow', $0]")>]
    let helper_overflow (_value: Overflow) : RawReactXPViewStyleRule = jsNative
    let overflow = fun v -> helper_overflow v

    // Text alignment
    [<RequireQualifiedAccess>]
    [<StringEnum>]
    type TextAlign = | Auto | Left | Right | Center | Justify

    [<RequireQualifiedAccess>]
    [<StringEnum>]
    type TextAlignVertical = | Auto | Top | Bottom | Center

    [<Emit("['textAlign', $0]")>]
    let helper_textAlign (_value: TextAlign) : RawReactXPTextStyleRule = jsNative
    let textAlign = fun v -> helper_textAlign v
    [<Emit("['textAlignVertical', $0]")>]
    let helper_textAlignVertical (_value: TextAlignVertical) : RawReactXPTextStyleRule = jsNative
    let textAlignVertical = fun v -> helper_textAlignVertical v

    // Text Style Attributes
    // Font Information

    // Attributes in this group cascade to child RX.Text components

    [<StringEnum>]
    type FontStyle = Normal | Italic
    [<StringEnum>]
    type FontWeight = Normal | Bold | [<CompiledName("100")>]W100 | [<CompiledName("200")>]W200 | [<CompiledName("300")>]W300 | [<CompiledName("400")>]W400 | [<CompiledName("500")>]W500 | [<CompiledName("600")>]W600 | [<CompiledName("700")>]W700 | [<CompiledName("800")>]W800 | [<CompiledName("900")>]W900


    [<Emit("['fontStyle', $0]")>]
    let helper_fontStyle (_value: FontStyle) : RawReactXPTextStyleRule = jsNative
    let fontStyle = fun v -> helper_fontStyle v
    [<Emit("['fontWeight', $0]")>]
    let helper_fontWeight (_value: FontWeight) : RawReactXPTextStyleRule = jsNative
    let fontWeight = fun v -> helper_fontWeight v

    [<StringEnum>]
    type Cursor = Pointer | Default // Web only

    [<Emit("['cursor', $0]")>]
    let helper_cursor (_value: Cursor) : RawReactXPViewStyleRule = jsNative
    let cursor = fun v -> helper_cursor v


[<AutoOpen>]
module RulesBasic =
    // Container layout
    [<Emit("['flex', $0]")>]
    let helper_flex (_value: int) : RawReactXPFlexStyleRule = jsNative
    let flex = fun v -> helper_flex v

    [<Emit("['flexGrow', $0]")>]
    let helper_flexGrow (_value: int) : RawReactXPFlexStyleRule = jsNative
    let flexGrow = fun v -> helper_flexGrow v

    [<Emit("['flexShrink', $0]")>]
    let helper_flexShrink (_value: int) : RawReactXPFlexStyleRule = jsNative
    let flexShrink = fun v -> helper_flexShrink v

    [<Emit("['flexBasis', $0]")>]
    let helper_flexBasis (_value: int) : RawReactXPFlexStyleRule = jsNative
    let flexBasis = fun v -> helper_flexBasis v

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
    let helper_height (_value: int) : RawReactXPFlexStyleRule = jsNative
    let height = fun v -> helper_height v
    [<Emit("['width', $0]")>]
    let helper_width (_value: int) : RawReactXPFlexStyleRule = jsNative
    let width = fun v -> helper_width v
    [<Emit("['maxHeight', $0]")>]
    let helper_maxHeight (_value: int) : RawReactXPFlexStyleRule = jsNative
    let maxHeight = fun v -> helper_maxHeight v
    [<Emit("['maxWidth', $0]")>]
    let helper_maxWidth (_value: int) : RawReactXPFlexStyleRule = jsNative
    let maxWidth = fun v -> helper_maxWidth v
    [<Emit("['minHeight', $0]")>]
    let helper_minHeight (_value: int) : RawReactXPFlexStyleRule = jsNative
    let minHeight = fun v -> helper_minHeight v
    [<Emit("['minWidth', $0]")>]
    let helper_minWidth (_value: int) : RawReactXPFlexStyleRule = jsNative
    let minWidth = fun v -> helper_minWidth v

    [<Emit("['height', ($0).toFixed(2) + '%']")>]
    let helper_height_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let heightPercent = fun v -> helper_height_percent v
    [<Emit("['width', ($0).toFixed(2) + '%']")>]
    let helper_width_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let widthPercent = fun v -> helper_width_percent v
    [<Emit("['maxHeight', ($0).toFixed(2) + '%']")>]
    let helper_maxHeight_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let maxHeightPercent = fun v -> helper_maxHeight_percent v
    [<Emit("['maxWidth', ($0).toFixed(2) + '%']")>]
    let helper_maxWidth_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let maxWidthPercent = fun v -> helper_maxWidth_percent v
    [<Emit("['minHeight', ($0).toFixed(2) + '%']")>]
    let helper_minHeight_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let minHeightPercent = fun v -> helper_minHeight_percent v
    [<Emit("['minWidth', ($0).toFixed(2) + '%']")>]
    let helper_minWidth_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let minWidthPercent = fun v -> helper_minWidth_percent v

    [<Emit("['height', $0]")>]
    let helper_animatedHeight (_value: AnimatableValue) : RawReactXPAnimatedFlexStyleRule = jsNative
    let animatedHeight = fun v -> helper_animatedHeight v
    [<Emit("['width', $0]")>]
    let helper_animatedWidth (_value: AnimatableValue) : RawReactXPAnimatedFlexStyleRule = jsNative
    let animatedWidth = fun v -> helper_animatedWidth v

    // Position Overrides

    type Position =
        static member Relative = RulesRestricted.position RulesRestricted.Position.Relative
        static member Absolute = RulesRestricted.position RulesRestricted.Position.Absolute

    [<Emit("['top', $0]")>]
    let helper_top (_value: int) : RawReactXPFlexStyleRule = jsNative
    let top = fun v -> helper_top v
    [<Emit("['right', $0]")>]
    let helper_right (_value: int) : RawReactXPFlexStyleRule = jsNative
    let right = fun v -> helper_right v
    [<Emit("['bottom', $0]")>]
    let helper_bottom (_value: int) : RawReactXPFlexStyleRule = jsNative
    let bottom = fun v -> helper_bottom v
    [<Emit("['left', $0]")>]
    let helper_left (_value: int) : RawReactXPFlexStyleRule = jsNative
    let left = fun v -> helper_left v

    [<Emit("['top', $0]")>]
    let helper_animatedTop (_value: AnimatableValue) : RawReactXPAnimatedFlexStyleRule = jsNative
    let animatedTop = fun v -> helper_animatedTop v
    [<Emit("['right', $0]")>]
    let helper_animatedRight (_value: AnimatableValue) : RawReactXPAnimatedFlexStyleRule = jsNative
    let animatedRight = fun v -> helper_animatedRight v
    [<Emit("['bottom', $0]")>]
    let helper_animatedBottom (_value: AnimatableValue) : RawReactXPAnimatedFlexStyleRule = jsNative
    let animatedBottom = fun v -> helper_animatedBottom v
    [<Emit("['left', $0]")>]
    let helper_animatedLeft (_value: AnimatableValue) : RawReactXPAnimatedFlexStyleRule = jsNative
    let animatedLeft = fun v -> helper_animatedLeft v

    // Margins

    [<Emit("['margin', $0]")>]
    let helper_margin (_value: int) : RawReactXPFlexStyleRule = jsNative
    let margin = fun v -> helper_margin v
    [<Emit("['marginHorizontal', $0]")>]
    let helper_marginHorizontal (_value: int) : RawReactXPFlexStyleRule = jsNative
    let marginHorizontal = fun v -> helper_marginHorizontal v
    [<Emit("['marginVertical', $0]")>]
    let helper_marginVertical (_value: int) : RawReactXPFlexStyleRule = jsNative
    let marginVertical = fun v -> helper_marginVertical v
    [<Emit("['marginTop', $0]")>]
    let helper_marginTop (_value: int) : RawReactXPFlexStyleRule = jsNative
    let marginTop = fun v -> helper_marginTop v
    [<Emit("['marginRight', $0]")>]
    let helper_marginRight (_value: int) : RawReactXPFlexStyleRule = jsNative
    let marginRight = fun v -> helper_marginRight v
    [<Emit("['marginBottom', $0]")>]
    let helper_marginBottom (_value: int) : RawReactXPFlexStyleRule = jsNative
    let marginBottom = fun v -> helper_marginBottom v
    [<Emit("['marginLeft', $0]")>]
    let helper_marginLeft (_value: int) : RawReactXPFlexStyleRule = jsNative
    let marginLeft = fun v -> helper_marginLeft v

    [<Emit("['margin', ($0).toFixed(2) + '%']")>]
    let helper_margin_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let marginPercent = fun v -> helper_margin_percent v
    [<Emit("['marginHorizontal', ($0).toFixed(2) + '%']")>]
    let helper_marginHorizontal_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let marginHorizontalPercent = fun v -> helper_marginHorizontal_percent v
    [<Emit("['marginVertical', ($0).toFixed(2) + '%']")>]
    let helper_marginVertical_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let marginVerticalPercent = fun v -> helper_marginVertical_percent v
    [<Emit("['marginTop', ($0).toFixed(2) + '%']")>]
    let helper_marginTop_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let marginTopPercent = fun v -> helper_marginTop_percent v
    [<Emit("['marginRight', ($0).toFixed(2) + '%']")>]
    let helper_marginRight_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let marginRightPercent = fun v -> helper_marginRight_percent v
    [<Emit("['marginBottom', ($0).toFixed(2) + '%']")>]
    let helper_marginBottom_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let marginBottomPercent = fun v -> helper_marginBottom_percent v
    [<Emit("['marginLeft', ($0).toFixed(2) + '%']")>]
    let helper_marginLeft_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let marginLeftPercent = fun v -> helper_marginLeft_percent v

    // Gaps

    [<Emit("['gap', $0]")>]
    let helper_gap (_value: int) : RawReactXPFlexStyleRule = jsNative
    let gap = fun v -> helper_gap v
    [<Emit("['rowGap', $0]")>]
    let helper_rowGap (_value: int) : RawReactXPFlexStyleRule = jsNative
    let rowGap = fun v -> helper_rowGap v
    [<Emit("['columnGap', $0]")>]
    let helper_columnGap (_value: int) : RawReactXPFlexStyleRule = jsNative
    let columnGap = fun v -> helper_columnGap v

    [<Emit("['gap', ($0).toFixed(2) + '%']")>]
    let helper_gap_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let gapPercent = fun v -> helper_gap_percent v
    [<Emit("['rowGap', ($0).toFixed(2) + '%']")>]
    let helper_rowGap_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let rowGapPercent = fun v -> helper_rowGap_percent v
    [<Emit("['columnGap', ($0).toFixed(2) + '%']")>]
    let helper_columnGap_percent (_value: float) : RawReactXPFlexStyleRule = jsNative
    let columnGapPercent = fun v -> helper_columnGap_percent v

    // Padding

    [<Emit("['padding', $0]")>]
    let helper_padding (_value: int) : RawReactXPFlexStyleRule = jsNative
    let padding = fun v -> helper_padding v
    [<Emit("['paddingHorizontal', $0]")>]
    let helper_paddingHorizontal (_value: int) : RawReactXPFlexStyleRule = jsNative
    let paddingHorizontal = fun v -> helper_paddingHorizontal v
    [<Emit("['paddingVertical', $0]")>]
    let helper_paddingVertical (_value: int) : RawReactXPFlexStyleRule = jsNative
    let paddingVertical = fun v -> helper_paddingVertical v
    [<Emit("['paddingTop', $0]")>]
    let helper_paddingTop (_value: int) : RawReactXPFlexStyleRule = jsNative
    let paddingTop = fun v -> helper_paddingTop v
    [<Emit("['paddingRight', $0]")>]
    let helper_paddingRight (_value: int) : RawReactXPFlexStyleRule = jsNative
    let paddingRight = fun v -> helper_paddingRight v
    [<Emit("['paddingBottom', $0]")>]
    let helper_paddingBottom (_value: int) : RawReactXPFlexStyleRule = jsNative
    let paddingBottom = fun v -> helper_paddingBottom v
    [<Emit("['paddingLeft', $0]")>]
    let helper_paddingLeft (_value: int) : RawReactXPFlexStyleRule = jsNative
    let paddingLeft = fun v -> helper_paddingLeft v


    // View Style Attributes

    // Color & Opacity

    [<StringEnum>]
    type AcrylicSourceUWP = Host | App

    [<Emit("['backgroundColor', $0]")>]
    let private helper_backgroundColorString (_value: string) : RawReactXPViewStyleRule = jsNative
    let private backgroundColorString = fun v -> helper_backgroundColorString v
    let backgroundColor (value: Color) : RawReactXPViewStyleRule =
        backgroundColorString value.ToReactXPString

    [<Emit("['opacity', $0]")>]
    let helper_opacity (_value: float) : RawReactXPViewStyleRule = jsNative
    let opacity = fun v -> helper_opacity v
    [<Emit("['acrylicOpacityUWP', $0]")>]
    let helper_acrylicOpacityUWP (_value: float) : RawReactXPViewStyleRule = jsNative
    let acrylicOpacityUWP = fun v -> helper_acrylicOpacityUWP v
    [<Emit("['acrylicSourceUWP', $0]")>]
    let helper_acrylicSourceUWP (_value: AcrylicSourceUWP) : RawReactXPViewStyleRule = jsNative
    let acrylicSourceUWP = fun v -> helper_acrylicSourceUWP v
    [<Emit("['acrylicTintColorUWP', $0]")>]
    let helper_acrylicTintColorUWP (_value: string) : RawReactXPViewStyleRule = jsNative
    let acrylicTintColorUWP = fun v -> helper_acrylicTintColorUWP v

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
    let helper_borderWidth (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderWidth = fun v -> helper_borderWidth v
    [<Emit("['borderTopWidth', $0]")>]
    let helper_borderTopWidth (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderTopWidth = fun v -> helper_borderTopWidth v
    [<Emit("['borderRightWidth', $0]")>]
    let helper_borderRightWidth (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderRightWidth = fun v -> helper_borderRightWidth v
    [<Emit("['borderBottomWidth', $0]")>]
    let helper_borderBottomWidth (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderBottomWidth = fun v -> helper_borderBottomWidth v
    [<Emit("['borderLeftWidth', $0]")>]
    let helper_borderLeftWidth (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderLeftWidth = fun v -> helper_borderLeftWidth v
    [<Emit("['borderStyle', $0]")>]
    let helper_borderStyle (_value: BorderStyle) : RawReactXPViewStyleRule = jsNative
    let borderStyle = fun v -> helper_borderStyle v
    [<Emit("['borderRadius', $0]")>]
    let helper_borderRadius (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderRadius = fun v -> helper_borderRadius v
    [<Emit("['borderTopRightRadius', $0]")>]
    let helper_borderTopRightRadius (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderTopRightRadius = fun v -> helper_borderTopRightRadius v
    [<Emit("['borderBottomRightRadius', $0]")>]
    let helper_borderBottomRightRadius (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderBottomRightRadius = fun v -> helper_borderBottomRightRadius v
    [<Emit("['borderBottomLeftRadius', $0]")>]
    let helper_borderBottomLeftRadius (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderBottomLeftRadius = fun v -> helper_borderBottomLeftRadius v
    [<Emit("['borderTopLeftRadius', $0]")>]
    let helper_borderTopLeftRadius (_value: int) : RawReactXPViewStyleRule = jsNative
    let borderTopLeftRadius = fun v -> helper_borderTopLeftRadius v


    [<Emit("['borderColor', $0]")>]
    let private helper_borderColorString (_value: string) : RawReactXPViewStyleRule = jsNative
    let private borderColorString = fun v -> helper_borderColorString v

    let borderColor (value: Color) : RawReactXPViewStyleRule =
        borderColorString value.ToReactXPString

    // Shadows

    // NOTE: If applied to a Text element, these properties translate to text shadows,
    // not a box shadow.
    type ShadowOffset = {
        height: int
        width:  int
    }
    [<Emit("['shadowOffset', $0]")>]
    let helper_shadowOffset (_value: ShadowOffset) : RawReactXPViewStyleRule = jsNative
    let shadowOffset = fun v -> helper_shadowOffset v
    [<Emit("['shadowRadius', $0]")>]
    let helper_shadowRadius (_value: int) : RawReactXPViewStyleRule = jsNative
    let shadowRadius = fun v -> helper_shadowRadius v
    [<Emit("['shadowOpacity', $0]")>]
    let helper_shadowOpacity (_value: int) : RawReactXPViewStyleRule = jsNative
    let shadowOpacity = fun v -> helper_shadowOpacity v
    [<Emit("['shadowColor', $0]")>]
    let private helper_shadowColorString (_value: string) : RawReactXPViewStyleRule = jsNative
    let private shadowColorString = fun v -> helper_shadowColorString v

    let shadowColor (value: Color) : RawReactXPViewStyleRule =
        shadowColorString value.ToReactXPString

    [<Emit("['elevation', $0]")>]
    let helper_elevation (_value: int) : RawReactXPViewStyleRule = jsNative
    let elevation = fun v -> helper_elevation v

    // Miscellaneous
    [<StringEnum>]
    type WordBreak =
    | [<CompiledName("break-all")>]BreakAll
    | [<CompiledName("break-word")>]BreakWord // Web only
    [<StringEnum>]
    type AppRegion = Drag | [<CompiledName("no-drag")>]NoDrag // Web only

    [<Emit("['wordBreak', $0]")>]
    let helper_wordBreak (_value: WordBreak) : RawReactXPViewStyleRule = jsNative
    let wordBreak = fun v -> helper_wordBreak v
    [<Emit("['appRegion', $0]")>]
    let helper_appRegion (_value: AppRegion) : RawReactXPViewStyleRule = jsNative
    let appRegion = fun v -> helper_appRegion v

    type Cursor =
        static member Pointer = RulesRestricted.cursor RulesRestricted.Cursor.Pointer
        static member Default = RulesRestricted.cursor RulesRestricted.Cursor.Default


    // Animated View Style Attributes
    [<Emit("['borderRadius', $0]")>]
    let helper_animated_border_radius (_value: obj) : RawReactXPAnimatedViewStyleRule = jsNative
    let animatedBorderRadius = fun (v: AnimatableValue) -> helper_animated_border_radius v.Raw
    [<Emit("['backgroundColor', $0]")>]
    let helper_animated_background_color (_value: obj) : RawReactXPAnimatedViewStyleRule = jsNative
    let animatedBackgroundColor = fun (v: InterpolatedValue) -> helper_animated_background_color v.Raw
    [<Emit("['opacity', $0]")>]
    let helper_animated_opacity (_value: obj) : RawReactXPAnimatedViewStyleRule = jsNative
    let animatedOpacity = fun (v: AnimatableValue) -> helper_animated_opacity v.Raw


    // Transform Style Attributes
    // Transforms

    type TransformValue =
    | HackingForNiceSyntaxAndReasonableTypeSafety

    type AnimatedTransformValue =
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
    let helper__transformHelper (_value: array<obj>) : RawReactXPTransformStyleRule = jsNative
    let _transformHelper = fun v -> helper__transformHelper v

    let transform (value: List<List<TransformValue>>) : RawReactXPTransformStyleRule =
        value
        |> List.map (fun (v) -> createObj (v :> obj :?> List<string * obj>))
        |> Array.ofList
        |> _transformHelper

    let animatedPerspective (value: AnimatableValue) : AnimatedTransformValue = ("perspective", value.Raw) :> obj :?> AnimatedTransformValue
    let animatedRotate (value: AnimatableValue) : AnimatedTransformValue = ("rotate", value.Raw) :> obj :?> AnimatedTransformValue
    let animatedRotateX (value: AnimatableValue) : AnimatedTransformValue = ("rotateX", value.Raw) :> obj :?> AnimatedTransformValue
    let animatedRotateY (value: AnimatableValue) : AnimatedTransformValue = ("rotateY", value.Raw) :> obj :?> AnimatedTransformValue
    let animatedRotateZ (value: AnimatableValue) : AnimatedTransformValue = ("rotateZ", value.Raw) :> obj :?> AnimatedTransformValue
    let animatedScale (value: AnimatableValue) : AnimatedTransformValue = ("scale", value.Raw) :> obj :?> AnimatedTransformValue
    let animatedScaleX (value: AnimatableValue) : AnimatedTransformValue = ("scaleX", value.Raw) :> obj :?> AnimatedTransformValue
    let animatedScaleY (value: AnimatableValue) : AnimatedTransformValue = ("scaleY", value.Raw) :> obj :?> AnimatedTransformValue
    let animatedTranslateX (value: AnimatableValue) : AnimatedTransformValue = ("translateX", value.Raw) :> obj :?> AnimatedTransformValue
    let animatedTranslateY (value: AnimatableValue) : AnimatedTransformValue = ("translateY", value.Raw) :> obj :?> AnimatedTransformValue

    [<Emit("['transform', $0]")>]
    let helper__animatedTransformHelper (_value: array<obj>) : RawReactXPAnimatedTransformStyleRule = jsNative
    let _animatedTransformHelper = fun v -> helper__animatedTransformHelper v

    let animatedTransform (value: List<List<AnimatedTransformValue>>) : RawReactXPAnimatedTransformStyleRule =
        value
        |> List.map (fun (v) -> createObj (v :> obj :?> List<string * obj>))
        |> Array.ofList
        |> _animatedTransformHelper

    // Animation rules

    [<Emit("['opacity', $0]")>]
    let helper_aniOpacity (_value: AnimatedValue) : RawReactXPViewStyleRule = jsNative
    let aniOpacity = fun v -> helper_aniOpacity v

    [<Emit("['height', $0]")>]
    let helper_aniHeight (_value: AnimatedValue) : RawReactXPViewStyleRule = jsNative
    let aniHeight = fun v -> helper_aniHeight v

    // Transform Style Attributes
    // Transforms

    type AniTransformValue =
    | AniHackingForNiceSyntaxAndReasonableTypeSafety

    // All transform values are animatable
    let aniPerspective (value: AnimatedValue) : AniTransformValue = ("perspective", value) :> obj :?> AniTransformValue
    let aniRotate (value: AnimatedValue) : AniTransformValue = ("rotate", value) :> obj :?> AniTransformValue
    let aniRotateX (value: AnimatedValue) : AniTransformValue = ("rotateX", value) :> obj :?> AniTransformValue
    let aniRotateY (value: AnimatedValue) : AniTransformValue = ("rotateY", value) :> obj :?> AniTransformValue
    let aniRotateZ (value: AnimatedValue) : AniTransformValue = ("rotateZ", value) :> obj :?> AniTransformValue
    let aniScale (value: AnimatedValue) : AniTransformValue = ("scale", value) :> obj :?> AniTransformValue
    let aniScaleX (value: AnimatedValue) : AniTransformValue = ("scaleX", value) :> obj :?> AniTransformValue
    let aniScaleY (value: AnimatedValue) : AniTransformValue = ("scaleY", value) :> obj :?> AniTransformValue
    let aniTranslateX (value: AnimatedValue) : AniTransformValue = ("translateX", value) :> obj :?> AniTransformValue
    let aniTranslateY (value: AnimatedValue) : AniTransformValue = ("translateY", value) :> obj :?> AniTransformValue

    let aniTransform (value: List<List<AniTransformValue>>) : RawReactXPTransformStyleRule =
        value
        |> List.map (fun (v) -> createObj (v :> obj :?> List<string * obj>))
        |> Array.ofList
        |> _transformHelper


    // Text Style Attributes
    // Font Information

    // Attributes in this group cascade to child RX.Text components
    [<Emit("['fontFamily', $0]")>]
    let helper_fontFamily (_value: string) : RawReactXPTextStyleRule = jsNative
    let fontFamily = fun v -> helper_fontFamily v
    [<Emit("['fontSize', $0]")>]
    let helper_fontSize (_value: int) : RawReactXPTextStyleRule = jsNative
    let fontSize = fun v -> helper_fontSize v

    [<Emit("['fontSize', $0]")>]
    let helper_animated_fontSize (_value: obj) : RawReactXPAnimatedTextStyleRule = jsNative
    let animatedFontSize = fun (v: AnimatableValue) -> helper_animated_fontSize v.Raw

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
    let private helper_colorString (_value: string) : RawReactXPTextStyleRule = jsNative
    let private colorString = fun v -> helper_colorString v
    let color (value: Color) : RawReactXPTextStyleRule =
        colorString value.ToReactXPString

    [<Emit("['color', $0]")>]
    let helper_animated_color (_value: obj) : RawReactXPAnimatedTextStyleRule = jsNative
    let animatedColor = fun (v: InterpolatedValue) -> helper_animated_color v.Raw

    // Spacing Overrides

    // Attributes in this group cascade to child RX.Text components
    [<Emit("['letterSpacing', $0]")>]
    let helper_letterSpacing (_value: int) : RawReactXPTextStyleRule = jsNative
    let letterSpacing = fun v -> helper_letterSpacing v
    [<Emit("['lineHeight', $0]")>]
    let helper_lineHeight (_value: int) : RawReactXPTextStyleRule = jsNative
    let lineHeight = fun v -> helper_lineHeight v

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
    let helper_textDecorationLine (_value: TextDecorationLine) : RawReactXPTextStyleRule = jsNative
    let textDecorationLine = fun v -> helper_textDecorationLine v
    [<Emit("['textDecorationStyle', $0]")>]
    let helper_textDecorationStyle (_value: TextDecorationStyle) : RawReactXPTextStyleRule = jsNative
    let textDecorationStyle = fun v -> helper_textDecorationStyle v
    [<Emit("['textDecorationColor', $0]")>]
    let helper_textDecorationColor (_value: string) : RawReactXPTextStyleRule = jsNative
    let textDecorationColor = fun v -> helper_textDecorationColor v

    // Writing Direction

    [<StringEnum>]
    type WritingDirection = Auto | Ltr | Rtl

    [<Emit("['writingDirection', $0]")>]
    let helper_writingDirection (_value: WritingDirection) : RawReactXPTextStyleRule = jsNative
    let writingDirection = fun v -> helper_writingDirection v

    // Miscellaneous

    [<Emit("['includeFontPadding', $0]")>]
    let helper_includeFontPadding (_value: bool) : RawReactXPTextStyleRule = jsNative
    let includeFontPadding = fun v -> helper_includeFontPadding v

