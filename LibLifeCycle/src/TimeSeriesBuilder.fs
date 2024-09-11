namespace LibLifeCycle

open LibLifeCycle
open Microsoft.FSharp.Core

#nowarn "1240" // ignores This type test or downcast will ignore the unit-of-measure 'UnitOfMeasure in AssumeTypes<>

type TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role
        when 'TimeSeriesDataPoint  :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
        and  'TimeSeriesId         :> TimeSeriesId<'TimeSeriesId>
        and  'OpError              :> OpError
        and  'TimeSeriesIndex      :> TimeSeriesIndex<'TimeSeriesIndex>
        and  'AccessPredicateInput :> AccessPredicateInput
        and  'Role                 :  comparison> =
    internal {
        Def: TimeSeriesDef<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>
        MaybeApiAccess: Option<TimeSeriesApiAccess<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'AccessPredicateInput, 'Session, 'Role>>
        Transform: TimeSeriesTransform<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError>
        Indices: 'TimeSeriesDataPoint -> seq<'TimeSeriesIndex>
    }
with
    member internal this.AssumeTypes<'NewAccessPredicateInput, 'NewRole
            when 'NewAccessPredicateInput :> AccessPredicateInput
            and  'NewRole                 :  comparison>()
            : TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'NewAccessPredicateInput, 'Session, 'NewRole> =
        {
            // All these fields have generic types that can be "filled in" as the builder is used, so we make a best effort to
            // retain existing values if their types match. Otherwise, those values are dropped.
            MaybeApiAccess =
                match this.MaybeApiAccess |> box with
                | :? (Option<TimeSeriesApiAccess<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'NewAccessPredicateInput, 'Session, 'NewRole>>) as existing -> existing
                | _ -> None
            Transform = this.Transform
            Indices = this.Indices
            Def = this.Def
        }

    member internal this.ToTimeSeries(): TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role> =
        {
            Definition  = this.Def
            Transform = this.Transform
            Indices = this.Indices
            MaybeApiAccess = this.MaybeApiAccess
            SessionHandling = None
        }

[<RequireQualifiedAccess>]
module TimeSeriesBuilder =
    let newTimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, [<Measure>] 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'Session, 'Role
                    when 'TimeSeriesDataPoint :> TimeSeriesDataPoint<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure>
                    and  'OpError :> OpError
                    and  'TimeSeriesIndex :> TimeSeriesIndex<'TimeSeriesIndex>
                    and  'Role    :  comparison>
            (def: TimeSeriesDef<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex>)
            : TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, AccessPredicateInput, 'Session, 'Role> =
        {
            Def = def
            MaybeApiAccess = None
            Transform = fun _ ts -> Ok ts
            Indices = fun _ -> Seq.empty
        }

    let withoutApiAccess
            (builder: TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role>)
            : TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role> =
        { builder with
            MaybeApiAccess = None
        }

    let withApiAccessRestrictedByRules
            (accessRules: List<TimeSeriesAccessRule<AccessPredicateInput, 'Role>>)
            (builder: TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'OldAccessPredicateInput, 'Session, 'Role>)
            : TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, AccessPredicateInput, 'Session, 'Role> =
        { builder.AssumeTypes<AccessPredicateInput, 'Role>() with
            MaybeApiAccess =
                Some {
                    AccessRules = accessRules
                    AccessPredicate = (fun _ _ _ _ -> true)
                }
        }

    let withApiAccessRestrictedByRulesAndPredicate
            (accessRules: List<TimeSeriesAccessRule<'AccessPredicateInput, 'Role>>)
            (accessPredicate: TimeSeriesAccessPredicate<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'AccessPredicateInput, 'Session>)
            (builder: TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'OldAccessPredicateInput, 'Session, 'Role>)
            : TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role>() with
            MaybeApiAccess =
                Some {
                    AccessRules = accessRules
                    AccessPredicate = accessPredicate
            }
        }

    let withApiAccessRestrictedToRootOnly
            (builder: TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role>)
            : TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, AccessPredicateInput, 'Session, 'Role> =
        builder
        |> withApiAccessRestrictedByRules []

    let withTransform
            (transform: TimeSeriesTransform<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError>)
            (builder: TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role>)
            : TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role>() with
            Transform =  transform }

    let withIndices
            (indices: 'TimeSeriesDataPoint -> seq<'TimeSeriesIndex>)
            (builder: TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role>)
            : TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role> =
        { builder.AssumeTypes<'AccessPredicateInput, 'Role>() with
            Indices = indices }

    let build
            (builder: TimeSeriesBuilder<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role>)
            : TimeSeries<'TimeSeriesDataPoint, 'TimeSeriesId, 'UnitOfMeasure, 'OpError, 'TimeSeriesIndex, 'AccessPredicateInput, 'Session, 'Role> =
        builder.ToTimeSeries()
