namespace LibLifeCycle

type ReferencedViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role
        when 'OpError              :> OpError
        and  'AccessPredicateInput :> AccessPredicateInput
        and  'Role                 :  comparison> =
    internal {
        Def:            ViewDef<'Input, 'Output, 'OpError>
        MaybeApiAccess: Option<ViewApiAccess<'Input, 'AccessPredicateInput, 'Session, 'Role>>
    }
with
    member internal this.AssumeTypes<'NewAccessPredicateInput, 'NewRole
            when 'NewAccessPredicateInput :> AccessPredicateInput
            and  'NewRole                 :  comparison>()
            : ReferencedViewBuilder<'Input, 'Output, 'OpError, 'NewAccessPredicateInput, 'Session, 'NewRole> =
        {
            // All these fields have generic types that can be "filled in" as the builder is used, so we make a best effort to
            // retain existing values if their types match. Otherwise, those values are dropped.
            MaybeApiAccess =
                match this.MaybeApiAccess |> box with
                | :? (Option<ViewApiAccess<'Input, 'NewAccessPredicateInput, 'Session, 'NewRole>>) as existing -> existing
                | _ -> None

            Def = this.Def
        }

    member internal this.ToReferencedView(): ReferencedView<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role> =
        {
            Def = this.Def
            MaybeApiAccess = this.MaybeApiAccess
        }

[<RequireQualifiedAccess>]
module ReferencedViewBuilder =
    let newReferencedView<'Input, 'Output, 'OpError, 'Session, 'Role
                when 'OpError              :> OpError
                and  'Role                 :  comparison>
            (def: ViewDef<'Input, 'Output, 'OpError>)
            : ReferencedViewBuilder<'Input, 'Output, 'OpError, AccessPredicateInput, 'Session, 'Role> =

        {
            Def = def
            MaybeApiAccess = None
        }

    let withoutApiAccess
            (builder: ReferencedViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role>)
            : ReferencedViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role> =
        { builder with
            MaybeApiAccess = None
        }

    let withApiAccessRestrictedByRules
            (accessRules: List<AccessRule<AccessPredicateInput, 'Role>>)
            (builder: ReferencedViewBuilder<'Input, 'Output, 'OpError, 'OldAccessPredicateInput, 'Session, 'OldRole>)
            : ReferencedViewBuilder<'Input, 'Output, 'OpError, AccessPredicateInput, 'Session, 'Role> =
        { builder.AssumeTypes<AccessPredicateInput, 'Role>() with
            MaybeApiAccess =
                Some {
                    AccessRules = accessRules
                    AccessPredicate = (fun _ _ _ _ -> true)
                }
        }

    let withApiAccessRestrictedByRulesAndPredicate
            (accessRules: List<AccessRule<'AccessPredicateInput, 'Role>>)
            (accessPredicate: AccessPredicate<'Input, 'AccessPredicateInput, 'Session>)
            (builder: ReferencedViewBuilder<'Input, 'Output, 'OpError, 'OldAccessPredicateInput, 'Session, 'Role>)
            : ReferencedViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role>() with
            MaybeApiAccess =
                Some {
                    AccessRules = accessRules
                    AccessPredicate = accessPredicate
            }
        }

    let withApiAccessRestrictedToRootOnly
            (builder: ReferencedViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role>)
            : ReferencedViewBuilder<'Input, 'Output, 'OpError, AccessPredicateInput, 'Session, 'Role> =
        builder
        |> withApiAccessRestrictedByRules []

    let build
            (builder: ReferencedViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role>)
            : ReferencedView<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role> =
        builder.ToReferencedView()
