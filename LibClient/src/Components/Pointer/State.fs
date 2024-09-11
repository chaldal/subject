[<AutoOpen>]
module LibClient.Components.Pointer_State

open Fable.React
open LibClient

module LC =
    module Pointer =
        module State =
            type PointerState = {
                IsHovered:      bool
                IsDepressed:    bool
                SetIsHovered:   bool -> Browser.Types.PointerEvent -> unit
                SetIsDepressed: bool -> Browser.Types.PointerEvent -> unit
            }

open LC.Pointer.State

type LibClient.Components.Constructors.LC.Pointer with
    [<Component>]
    static member State(content: PointerState -> ReactElement) : ReactElement =
        let isHoveredState = Hooks.useState false
        let isDepressedState = Hooks.useState false

        let pointerState =
            {
                IsHovered = isHoveredState.current
                IsDepressed = isDepressedState.current
                SetIsHovered =
                    fun value (_: Browser.Types.PointerEvent) ->
                        isHoveredState.update value
                        // Restoring the Depressed behaviour when pointer is moved out is
                        // supposed to work according to ReactXP docs, but doesn't seem to.
                        // This fixes at least the "moved out" part. Can't think of a simple
                        // fix for the "moved in again" part, so leaving that out for now.
                        isDepressedState.update (if not value then false else isDepressedState.current)
                SetIsDepressed =
                    fun value (_: Browser.Types.PointerEvent) ->
                        isDepressedState.update value
            }

        content pointerState
