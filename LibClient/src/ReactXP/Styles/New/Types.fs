namespace ReactXP.Styles

module Config =
    let mutable private isDevMode: bool = false
    let getIsDevMode () : bool = isDevMode
    let setIsDevMode (value: bool) =
        isDevMode <- value

[<RequireQualifiedAccess>]
type RawReactXPStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

[<RequireQualifiedAccess>]
type RawReactXPViewStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

[<RequireQualifiedAccess>]
type RawReactXPAnimatedViewStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

[<RequireQualifiedAccess>]
type RawReactXPTextStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

[<RequireQualifiedAccess>]
type RawReactXPAnimatedTextStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

[<RequireQualifiedAccess>]
type RawReactXPFlexChildrenStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

[<RequireQualifiedAccess>]
type RawReactXPFlexStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

[<RequireQualifiedAccess>]
type RawReactXPAnimatedFlexStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

[<RequireQualifiedAccess>]
type RawReactXPTransformStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

[<RequireQualifiedAccess>]
type RawReactXPAnimatedTransformStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

type Color = LibClient.ColorModule.Color
