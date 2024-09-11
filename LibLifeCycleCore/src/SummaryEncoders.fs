module LibLifeCycleCore.SummaryEncoders

open System

let getSummaryEncodersForLifeCycle (lifeCycle: LifeCycleDef) : seq<Type * (obj -> string)> =
    lifeCycle.Invoke
        { new FullyTypedLifeCycleDefFunction<_> with
            member _.Invoke (_def: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
                LibLifeCycleCore.OrleansEx.Serializer.getSummaryEncoders<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>() }