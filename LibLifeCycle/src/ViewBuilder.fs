namespace LibLifeCycle

open LibLifeCycle
open Microsoft.FSharp.Core

type ViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env
        when 'OpError              :> OpError
        and  'AccessPredicateInput :> AccessPredicateInput
        and  'Role                 :  comparison
        and  'Env                  :> Env> =
    internal {
        Def: ViewDef<'Input, 'Output, 'OpError>
        MaybeApiAccess: Option<ViewApiAccess<'Input, 'AccessPredicateInput, 'Session, 'Role>>
        Read: Option<Read<'Input, 'Output, 'OpError, 'Env>>
    }
with
    member internal this.AssumeTypes<'NewAccessPredicateInput, 'NewRole, 'NewEnv
            when 'NewAccessPredicateInput :> AccessPredicateInput
            and  'NewRole                 :  comparison
            and  'NewEnv                  :> Env>()
            : ViewBuilder<'Input, 'Output, 'OpError, 'NewAccessPredicateInput, 'Session, 'NewRole, 'NewEnv> =
        {
            // All these fields have generic types that can be "filled in" as the builder is used, so we make a best effort to
            // retain existing values if their types match. Otherwise, those values are dropped.
            MaybeApiAccess =
                match this.MaybeApiAccess |> box with
                | :? (Option<ViewApiAccess<'Input, 'NewAccessPredicateInput, 'Session, 'NewRole>>) as existing -> existing
                | _ -> None
            Read =
                match this.Read |> box with
                | :? (Option<Read<'Input, 'Output, 'OpError, 'NewEnv>>) as existing -> existing
                | _ -> None

            Def = this.Def
        }

    member internal this.ToView(): View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        match this.Read with
        | Some read ->
            {
                Definition  = this.Def
                Read = read
                MetaData = { EnvironmentType_ = typeof<'Env> }
                MaybeApiAccess = this.MaybeApiAccess
                SessionHandling = None
            }
        | None ->
            failwith $"Must provide read logic via ViewBuilder.withRead when building view {this.Def.Name}"

[<RequireQualifiedAccess>]
module ViewBuilder =
    let newView<'Input, 'Output, 'OpError, 'Session, 'Role
                    when 'OpError :> OpError
                    and  'Role    :  comparison>
            (def: ViewDef<'Input, 'Output, 'OpError>)
            : ViewBuilder<'Input, 'Output, 'OpError, AccessPredicateInput, 'Session, 'Role, Env> =
        {
            Def = def
            MaybeApiAccess = None
            Read = None
        }

    let withoutApiAccess
            (builder: ViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : ViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder with
            MaybeApiAccess = None
        }

    let withApiAccessRestrictedByRules
            (accessRules: List<AccessRule<AccessPredicateInput, 'Role>>)
            (builder: ViewBuilder<'Input, 'Output, 'OpError, 'OldAccessPredicateInput, 'Session, 'Role, 'Env>)
            : ViewBuilder<'Input, 'Output, 'OpError, AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder.AssumeTypes<AccessPredicateInput, 'Role, 'Env>() with
            MaybeApiAccess =
                Some {
                    AccessRules = accessRules
                    AccessPredicate = (fun _ _ _ _ -> true)
                }
        }

    let withApiAccessRestrictedByRulesAndPredicate
            (accessRules: List<AccessRule<'AccessPredicateInput, 'Role>>)
            (accessPredicate: AccessPredicate<'Input, 'AccessPredicateInput, 'Session>)
            (builder: ViewBuilder<'Input, 'Output, 'OpError, 'OldAccessPredicateInput, 'Session, 'Role, 'Env>)
            : ViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role, 'Env>() with
            MaybeApiAccess =
                Some {
                    AccessRules = accessRules
                    AccessPredicate = accessPredicate
            }
        }

    let withApiAccessRestrictedToRootOnly
            (builder: ViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : ViewBuilder<'Input, 'Output, 'OpError, AccessPredicateInput, 'Session, 'Role, 'Env> =
        builder
        |> withApiAccessRestrictedByRules []

    let withRead
            (read: Read<'Input, 'Output, 'OpError, 'Env>)
            (builder: ViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'OldEnv>)
            : ViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role, 'Env>() with
            Read = Some read
        }

    let build
            (builder: ViewBuilder<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env>)
            : View<'Input, 'Output, 'OpError, 'AccessPredicateInput, 'Session, 'Role, 'Env> =
        builder.ToView()
