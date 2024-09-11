[<AutoOpen>]
module LibClient.Components.ReactHelpers

open Fable.React
open LibClient
open Fable.Core.JsInterop

type LibClient.Components.Constructors.LC with
    /// <summary>Wrap children in a fragment, optionally specifying a key</summary>
    /// <param name="children" type="array&lt;ReactElement&gt;"/>
    /// <param name="key" type="string" default="None"/>
    [<Component>]
    static member Fragment (children: array<ReactElement>, ?key: string) : ReactElement =
        Fable.React.Helpers.fragment
            [match key with Some value -> !!("key", value) | None -> ()]
            children
