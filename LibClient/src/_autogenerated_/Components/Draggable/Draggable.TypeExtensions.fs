namespace LibClient.Components

open LibClient
open System
open Fable.Core.JsInterop
open ReactXP.Components
open ReactXP.Styles.Animation
open ReactXP.Styles
open LibClient.Components.Draggable
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module DraggableTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member Draggable(?children: ReactChildrenProp, ?baseOffset: int * int, ?left: DragTarget, ?right: DragTarget, ?up: DragTarget, ?down: DragTarget, ?onChange: (Change -> unit), ?key: string, ?ref: (LibClient.JsInterop.JsNullable<IDraggableRef> -> unit), ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    BaseOffset = defaultArg baseOffset ((0, 0))
                    Left = left |> Option.orElse (None)
                    Right = right |> Option.orElse (None)
                    Up = up |> Option.orElse (None)
                    Down = down |> Option.orElse (None)
                    OnChange = onChange |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                    ref = ref |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Draggable.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            