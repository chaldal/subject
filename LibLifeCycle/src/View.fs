[<AutoOpen>]
module LibLifeCycle.View

open System
open LibLifeCycle
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection

let internal createEnvImpl<'Env> (callScopedEnvDependencies: CallScopedEnvDependencies) (serviceProvider: IServiceProvider) : 'Env =
    let environmentFactory = serviceProvider.GetRequiredService<EnvironmentFactory<'Env>>()
    let environment = environmentFactory.Invoke(callScopedEnvDependencies)
    environment

type FullyTypedViewFunction<'Res> =
    abstract member Invoke: View<_, _, _, _, _, _, _> -> 'Res

and IView =
    abstract member Name: string
    abstract member Def: IViewDef
    abstract member EnableApiAccess: bool
    abstract member Invoke: FullyTypedViewFunction<'Res> -> 'Res

and ViewResult<'Output, 'OpError when 'OpError :> OpError> = ViewResult of Task<Result<'Output, 'OpError>>

and ViewMetaData = internal {
    EnvironmentType_: Type
}
with
    member this.EnvironmentType = this.EnvironmentType_

and FullyTypedViewFunction<'Res, 'Input, 'Output, 'OpError when 'OpError :> OpError> =
    abstract member Invoke: View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env> -> 'Res

and IView<'Input, 'Output, 'OpError when 'OpError :> OpError> =
    inherit IView
    abstract member MetaData: ViewMetaData
    abstract member Invoke: FullyTypedViewFunction<'Res, 'Input, 'Output, 'OpError> -> 'Res

    // TODO: these also can be supplanted with function invocations (Invoke calls)
    abstract member Read: CallOrigin -> IServiceProvider -> 'Input -> ViewResult<'Output, 'OpError>

and Read<'Input, 'Output, 'OpError, 'Env
        when 'OpError :> OpError
        and  'Env     :> Env> =
    'Env -> 'Input -> ViewResult<'Output, 'OpError>

and AccessPredicate<'Input, 'AccessPredicateInput, 'Session
        when 'AccessPredicateInput :> AccessPredicateInput> =
    'AccessPredicateInput -> 'Input -> CallOrigin -> Option<'Session> -> bool

and ViewApiAccess<'Input, 'AccessPredicateInput, 'Session, 'Role
        when 'AccessPredicateInput :> AccessPredicateInput
        and  'Role                 :  comparison> = {
    AccessRules:     List<AccessRule<'AccessPredicateInput, 'Role>>
    AccessPredicate: AccessPredicate<'Input, 'AccessPredicateInput, 'Session>
}

and View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env
        when 'OpError              :> OpError
        and  'AccessPredicateInput :> AccessPredicateInput
        and  'Role                 :  comparison
        and  'Env                  :> Env> = internal {
    Read:            Read<'Input, 'Output, 'OpError, 'Env>
    MaybeApiAccess:  Option<ViewApiAccess<'Input, 'AccessPredicateInput, 'Session, 'Role>>
    MetaData:        ViewMetaData
    Definition:      ViewDef<'Input, 'Output, 'OpError>
    SessionHandling: Option<EcosystemSessionHandling<'Session, 'Role>>
}
with
    member this.Name = this.Definition.Name

    member internal _.CreateEnv (callOrigin: CallOrigin) (serviceProvider: IServiceProvider) =
        createEnvImpl<'Env>
            { CallOrigin = callOrigin
              LocalSubjectRef = None }
            serviceProvider

    interface IView<'Input, 'Output, 'OpError> with
        member this.Name = this.Name
        member this.Def = this.Definition
        member this.MetaData = this.MetaData
        member this.EnableApiAccess =
            this.MaybeApiAccess
            |> Option.map (fun _ -> true)
            |> Option.defaultValue false
        member this.Invoke (fn: FullyTypedViewFunction<_, _, _, _>) = fn.Invoke this
        member this.Invoke (fn: FullyTypedViewFunction<_>) = fn.Invoke this

        member this.Read callOrigin serviceProvider input =
            let env = this.CreateEnv callOrigin serviceProvider
            this.Read env input

and AccessRule<'AccessPredicateInput, 'Role
        when 'AccessPredicateInput :> AccessPredicateInput
        and  'Role                 :  comparison> = {
    Input:        AccessMatch<'AccessPredicateInput>
    Roles:        AccessMatch<NonemptySet<'Role>>
    Decision:     AccessDecision
}