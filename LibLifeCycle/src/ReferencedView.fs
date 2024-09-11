[<AutoOpen>]
module LibLifeCycle.ReferencedView

type FullyTypedReferencedViewFunction<'Res> =
    abstract member Invoke: ReferencedView<_, _, _, _, _, _> -> 'Res

and FullyTypedReferencedViewFunction<'Res, 'Input, 'Output, 'OpError when 'OpError :> OpError> =
    abstract member Invoke: ReferencedView<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role> -> 'Res

and IReferencedView =
    abstract member Name: string
    abstract member Def: IViewDef
    abstract member EnableApiAccess: bool
    abstract member Invoke: FullyTypedReferencedViewFunction<'Res> -> 'Res

and IReferencedView<'Input, 'Output, 'OpError when 'OpError :> OpError> =
    inherit IReferencedView
    abstract member Invoke: FullyTypedReferencedViewFunction<'Res, 'Input, 'Output, 'OpError> -> 'Res

and ReferencedView<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role
        when 'OpError              :> OpError
        and  'AccessPredicateInput :> AccessPredicateInput
        and  'Role                 :  comparison> =
    internal {
        Def:            ViewDef<'Input, 'Output, 'OpError>
        MaybeApiAccess: Option<ViewApiAccess<'Input, 'AccessPredicateInput, 'Session, 'Role>>
    }
with
    member this.Name = this.Def.Name

    interface IReferencedView<'Input, 'Output, 'OpError> with
        member this.Invoke (fn: FullyTypedReferencedViewFunction<_, _, _, _>) = fn.Invoke this
        member this.Invoke (fn: FullyTypedReferencedViewFunction<_>) = fn.Invoke this
        member this.Name = this.Name
        member this.Def = this.Def
        member this.EnableApiAccess =
            this.MaybeApiAccess
            |> Option.map (fun _ -> true)
            |> Option.defaultValue false
