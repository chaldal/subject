[<AutoOpen>]
module SuiteT__EC__T.LifeCycles.T__EC__TLifeCycleBuilder

open LibLifeCycle
open SuiteT__EC__T.Types

let newT__EC__TLifeCycle
    (def: LifeCycleDef<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId>) =
    LifeCycleBuilder.newLifeCycle<'Subject, 'LifeAction, 'OpError, 'Constructor, 'LifeEvent, 'SubjectIndex, 'SubjectId, NoSession, NoRole>
        def

let newT__EC__TView
    (def: ViewDef<'Input, 'Output, 'OpError>) =
    ViewBuilder.newView<'Input, 'Output, 'OpError, NoSession, NoRole>
        def