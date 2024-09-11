namespace LibClient.Components

open LibClient
open LibClient.Components.Legacy.Sidebar.Filler
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module Legacy_Sidebar_FillerTypeExtensions =
    type LibClient.Components.Constructors.LC.Legacy.Sidebar with
        static member Filler(?children: ReactChildrenProp, ?hackWeDoNotSupportProplessComponents: bool, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    HackWeDoNotSupportProplessComponents = defaultArg hackWeDoNotSupportProplessComponents (true)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Legacy.Sidebar.Filler.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            