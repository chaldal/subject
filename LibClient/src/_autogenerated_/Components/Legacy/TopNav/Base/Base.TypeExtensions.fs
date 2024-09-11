namespace LibClient.Components

open LibClient
open LibClient.Components.Legacy.TopNav.Base
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Legacy_TopNav_BaseTypeExtensions =
    type LibClient.Components.Constructors.LC.Legacy.TopNav with
        static member Base(center: Center, ?children: ReactChildrenProp, ?left: ReactElement, ?right: ReactElement, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Center = center
                    Left = defaultArg left (noElement)
                    Right = defaultArg right (noElement)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Legacy.TopNav.Base.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            