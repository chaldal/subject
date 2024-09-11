[<AutoOpen>]
module SuiteJobs.LifeCycles.JobsLifeCycleBuilder

open LibLifeCycle
open SuiteJobs.Types

let newJobsLifeCycle
    (def: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
    LifeCycleBuilder.newLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, NoSession, NoRole>
        def

let newJobsView
    (def: ViewDef<'Input, 'Output, 'OpError>) =
    ViewBuilder.newView<'Input, 'Output, 'OpError, NoSession, NoRole>
        def