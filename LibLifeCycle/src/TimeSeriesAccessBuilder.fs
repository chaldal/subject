module LibLifeCycle.TimeSeriesAccessBuilder

open LibLifeCycle

type TimeSeriesAccessEventBuilder =
    private TimeSeriesAccessEventBuilder of NonemptySet<TimeSeriesAccessEventType>

let grant (events: list<TimeSeriesAccessEventType>) : TimeSeriesAccessRule<_, _> =
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

let grantWhen input (events: list<TimeSeriesAccessEventType>) : TimeSeriesAccessRule<_, _> =
    { grant events with Input = Match input }

let deny (events: list<TimeSeriesAccessEventType>) : TimeSeriesAccessRule<_, _> =
    {
        Input =      MatchAny
        Roles =      MatchAny
        EventTypes = NonemptySet.ofList events |> Option.map Match |> Option.defaultValue MatchAny
        Decision =   Deny
    }

let denyWhen input (events: list<TimeSeriesAccessEventType>) : TimeSeriesAccessRule<_, _> =
    { deny events with Input = Match input }

let grantAll : TimeSeriesAccessRule<_, _> =
    {
        Input =      MatchAny
        Roles =      MatchAny
        EventTypes = MatchAny
        Decision =   Grant
    }

let denyAccess : TimeSeriesAccessRule<_, _> =
    {
        Input =      MatchAny
        Roles =      MatchAny
        EventTypes = MatchAny
        Decision =   Deny
    }

let toRole role accessRule : TimeSeriesAccessRule<_, _> =
    { accessRule with
        Roles =
            match accessRule.Roles with
            | MatchAny ->
                NonemptySet.ofOneItem role |> Match
            | Match roles ->
                roles.Add role |> Match
    }

let toRoles (roles: NonemptySet<'Role>) accessRule : TimeSeriesAccessRule<_, _> =
    { accessRule with
        Roles =
            match accessRule.Roles with
            | MatchAny ->
                roles |> Match
            | Match existingRoles ->
                NonemptySet.union roles existingRoles
                |> Match
    }