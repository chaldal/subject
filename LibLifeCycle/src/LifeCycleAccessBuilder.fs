module LibLifeCycle.LifeCycleAccessBuilder

open LibLifeCycle

type AccessEventBuilder<'LifeAction, 'Constructor when 'LifeAction :> LifeAction and 'Constructor :> Constructor> =
    private AccessEventBuilder of NonemptySet<AccessEventType<'LifeAction, 'Constructor>>

type AccessTo<'LifeAction, 'Constructor when 'LifeAction :> LifeAction and 'Constructor :> Constructor> =

    static member Read =
        AccessEventType<'LifeAction, 'Constructor>.Read (OriginalProjection)

    static member ReadHistory =
        AccessEventType<'LifeAction, 'Constructor>.ReadHistory

    static member ReadBlob =
        AccessEventType<'LifeAction, 'Constructor>.ReadBlob

    static member ReadProjection (projection: SubjectProjectionDef<'Projection, 'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
        AccessEventType<'LifeAction, 'Constructor>.Read (Projection projection.ProjectionName)

    static member Act (caseFactory: 'TupleOrSingleValue -> 'LifeAction) =
        caseFactory
        |> UnionCase.ofCase
        |> AccessEventType<'LifeAction, 'Constructor>.ActCase

    static member Act (action: 'LifeAction) =
        action
        |> UnionCase.ofCase
        |> AccessEventType<'LifeAction, 'Constructor>.ActCase

    static member Construct (caseFactory: 'TupleOrSingleValue -> 'Constructor) =
        caseFactory
        |> UnionCase.ofCase
        |> AccessEventType<'LifeAction, 'Constructor>.ConstructCase

    static member Construct (action: 'Constructor) =
        action
        |> UnionCase.ofCase
        |> AccessEventType<'LifeAction, 'Constructor>.ConstructCase

let grant (events: list<AccessEventType<'LifeAction, 'Constructor>>) =
    match NonemptySet.ofList events with
    | None -> // To be safe, convert this to a Deny
        {
            Input =      MatchAny
            Roles =      MatchAny
            EventTypes = MatchAny
            Decision =   Deny
        }
    | Some events ->
        {
            Input =      MatchAny
            Roles =      MatchAny
            EventTypes = Match events
            Decision =   Grant
        }

let grantWhen input (events: list<AccessEventType<'LifeAction, 'Constructor>>) =
    { grant events with Input = Match input }

let deny (events: list<AccessEventType<'LifeAction, 'Constructor>>) =
    {
        Input =      MatchAny
        Roles =      MatchAny
        EventTypes = NonemptySet.ofList events |> Option.map Match |> Option.defaultValue MatchAny
        Decision =   Deny
    }

let denyWhen input (events: list<AccessEventType<'LifeAction, 'Constructor>>) =
    { deny events with Input = Match input }

let grantAll =
    {
        Input =      MatchAny
        Roles =      MatchAny
        EventTypes = MatchAny
        Decision =   Grant
    }

let denyAccess =
    {
        Input =      MatchAny
        Roles =      MatchAny
        EventTypes = MatchAny
        Decision =   Deny
    }

let toRole role accessRule =
    { accessRule with
        Roles =
            match accessRule.Roles with
            | MatchAny ->
                NonemptySet.ofOneItem role |> Match
            | Match roles ->
                roles.Add role |> Match
    }

let toRoles (roles: NonemptySet<'Role>) accessRule =
    { accessRule with
        Roles =
            match accessRule.Roles with
            | MatchAny ->
                roles |> Match
            | Match existingRoles ->
                NonemptySet.union roles existingRoles
                |> Match
    }