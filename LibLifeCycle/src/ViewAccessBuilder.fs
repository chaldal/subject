module LibLifeCycle.ViewAccessBuilder

open LibLifeCycle

let grant =
    {
        Input =    MatchAny
        Roles =    MatchAny
        Decision = Grant
    }

let grantWhen input =
    { grant with Input = Match input }

let deny =
    {
        Input =    MatchAny
        Roles =    MatchAny
        Decision = Deny
    }

let denyWhen input =
    { deny with Input = Match input }

let toRole role (accessRule: AccessRule<'AccessPredicateInput, 'Role>) =
    { accessRule with
        Roles =
            match accessRule.Roles with
            | MatchAny ->
                NonemptySet.ofOneItem role |> Match
            | Match roles ->
                roles.Add role |> Match
    }

let toRoles (roles: NonemptySet<'Role>) (accessRule: AccessRule<'AccessPredicateInput, 'Role>) =
    { accessRule with
        Roles =
            match accessRule.Roles with
            | MatchAny ->
                roles |> Match
            | Match existingRoles ->
                NonemptySet.union roles existingRoles
                |> Match
    }