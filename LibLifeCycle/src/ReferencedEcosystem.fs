[<AutoOpen>]
module LibLifeCycle.ReferencedEcosystem

type ReferencedEcosystem =
    {
        Def:        EcosystemDef
        LifeCycles: List<IReferencedLifeCycle>
        Views:      List<IReferencedView>
        // TODO: TimeSeries / IReferencedTimeSeries
    }