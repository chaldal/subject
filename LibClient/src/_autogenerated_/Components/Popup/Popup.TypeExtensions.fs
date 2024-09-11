namespace LibClient.Components

open LibClient
open Fable.Core
open Fable.Core.JsInterop
open LibLang
open LibClient.Components.Popup
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module PopupTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member Popup(render: unit -> ReactElement, connector: Connector, ?children: ReactChildrenProp, ?id: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Render = render
                    Connector = connector
                    Id = id |> Option.orElse (None)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Popup.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            