module LibUiSubjectAdmin.Components.Audit.Types

type AuditSubjectId = {
    Name:     string
    IdString: string
}
with
    static member inline OfSubject (lifeCycleDef: LifeCycleDef<'Subject, _, _, _, _, _, 'Id>) (subject: 'Subject) : AuditSubjectId =
        {
            Name     = lifeCycleDef.Key.LocalLifeCycleName
            IdString = ((subject :> Subject<'Id>).SubjectId :> SubjectId).IdString
        }

    static member inline OfId (lifeCycleDef: LifeCycleDef<_, _, _, _, _, _, 'Id>) (id: 'Id) : AuditSubjectId =
        {
            Name     = lifeCycleDef.Key.LocalLifeCycleName
            IdString = (id :> SubjectId).IdString
        }
