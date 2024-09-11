module LibLifeCycleCore.OrleansEx.TraceContextGrainCallFilter

open LibLifeCycleCore.Anchor
open Orleans
open Orleans.Runtime

/// Id of parent activity for distributed tracing, in W3C format: 00-{CorrelationId}-{ItemId}-00
/// either passed manually between silos or captured from ambient context of native Orleans client
[<Literal>]
let ParentActivityIdKey = "ActivityParentId"

let private coreAssembly = typeof<AnchorTypeForProject>.Assembly

// make it internal so host doesn't use this filter by accident
type internal TraceContextOutgoingGrainCallFilter () =
    interface IOutgoingGrainCallFilter with
        member this.Invoke(context: IOutgoingGrainCallContext) =
            if context.InterfaceMethod = null || context.InterfaceMethod.DeclaringType.Assembly = coreAssembly then
                // pass parent activity Id from client calls to our grains
                // typical external activity is HTTP Request, so it will correlate with backend telemetry
                let activity = System.Diagnostics.Activity.Current
                RequestContext.Set (ParentActivityIdKey, if activity = null then null else activity.Id)
                context.Invoke()
            else
                context.Invoke()