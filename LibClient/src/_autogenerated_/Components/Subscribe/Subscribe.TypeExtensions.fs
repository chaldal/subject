namespace LibClient.Components

open LibClient
open LibClient.Services.Subscription
open Fable.React
open System
open LibClient.Components.Subscribe
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module SubscribeTypeExtensions =
    type LibClient.Components.Constructors.LC with
        static member Subscribe(subscription: (LibClient.AsyncDataModule.AsyncData<'T> -> unit) -> SubscribeResult, ``with``: With<'T>, ?children: ReactChildrenProp, ?onSubscriptionKeyChange: OnSubscriptionKeyChange, ?subscriptionKey: string, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Subscription = subscription
                    With = ``with``
                    OnSubscriptionKeyChange = defaultArg onSubscriptionKeyChange (OnSubscriptionKeyChange.ShowCurrentDataAsFetching)
                    SubscriptionKey = subscriptionKey |> Option.orElse (None)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibClient.Components.Subscribe.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            