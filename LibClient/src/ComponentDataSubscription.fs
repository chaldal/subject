module LibClient.ComponentDataSubscription

open LibClient
open LibClient.Services.Subscription

let wireUpSubscriptionToComponent (theComponent: PstatefulComponent<_, _, 'Pstate, _, _>) (subscription: Subscription<'D>) (pstateSetter: 'Pstate -> AsyncData<'D> -> 'Pstate) : unit =
    subscription.On (fun (data: AsyncData<'D>) -> theComponent.SetPstate (fun pstate _ _ -> pstateSetter pstate data))
    theComponent.RunOnUnmount subscription.Off

let wireUpAdHocSubscriptionToComponent (theComponent: Component<_, _>) (subscriptionResult: SubscribeResult) : unit =
    theComponent.RunOnUnmount subscriptionResult.Off
