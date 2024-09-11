namespace LibLifeCycle

type ReferencedEcosystemBuilder =
    internal {
        Def:        EcosystemDef
        LifeCycles: List<IReferencedLifeCycle>
        Views:      List<IReferencedView>
    }
with
    member this.ToReferencedEcosystem()
            : ReferencedEcosystem =
        // We need to ensure all life cycles that were not explicitly referenced are referenced anyway, albeit without API
        // access. This is to ensure that the native ecosystem can yield side effects for all foreign life cycles regardless
        // of an explicit reference, since that is the behavior permitted within simulations.
        let ecosystemLifeCycleDefsByKey =
            this.Def.LifeCycleDefs
            |> List.filter (fun lifeCycleDef -> lifeCycleDef.LifeCycleKey.LocalLifeCycleName <> MetaLifeCycleName)
            |> List.map (fun lifeCycleDef -> lifeCycleDef.LifeCycleKey, lifeCycleDef)
            |> Map.ofList

        let explicitlyReferencedLifeCycleKeys =
            this.LifeCycles
            |> List.map (fun referencedLifeCycle -> referencedLifeCycle.Def.LifeCycleKey)
            |> Set.ofList

        let implicitlyReferencedLifeCycles =
            explicitlyReferencedLifeCycleKeys
            |> Set.difference ecosystemLifeCycleDefsByKey.KeySet
            |> Set.toList
            |> List.map (fun lifeCycleKey -> ecosystemLifeCycleDefsByKey |> Map.find lifeCycleKey)
            |> List.map (fun lifeCycleDef ->
                lifeCycleDef.Invoke
                    { new FullyTypedLifeCycleDefFunction<_> with
                        member _.Invoke (lifeCycleDef: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
                            lifeCycleDef
                            |> ReferencedLifeCycleBuilder.newReferencedLifeCycle
                            |> ReferencedLifeCycleBuilder.withoutApiAccess
                            |> ReferencedLifeCycleBuilder.build
                            :> IReferencedLifeCycle
                    }
            )

        {
            Def = this.Def
            LifeCycles = this.LifeCycles @ implicitlyReferencedLifeCycles
            Views = this.Views
        }

[<RequireQualifiedAccess>]
module ReferencedEcosystemBuilder =
    let newReferencedEcosystem
            (def: EcosystemDef)
            : ReferencedEcosystemBuilder =
        {
            Def = def
            LifeCycles = List.empty
            Views = List.empty
        }

    let addLifeCycle
            (lifeCycle: IReferencedLifeCycle)
            (builder: ReferencedEcosystemBuilder)
            : ReferencedEcosystemBuilder =
        { builder with
            LifeCycles = lifeCycle :: builder.LifeCycles
        }

    // TODO: support referenced views (requires 'Input and 'Output codecs, which will be a breaking change for existing views using
    // primitives for input/output)
    //let addView
    //        (view: IReferencedView)
    //        (builder: ReferencedEcosystemBuilder)
    //        : ReferencedEcosystemBuilder =
    //    { builder with
    //        Views = view :: builder.Views
    //    }

    let build
            (builder: ReferencedEcosystemBuilder)
            : ReferencedEcosystem =
        builder.ToReferencedEcosystem()