SELECT
    '[___LIFECYCLE_NAME___]' AS Subj,
    SubjectId,
    [___SCHEMA_NAME___].decode(SideEffect) AS SideEffect,
    SideEffectId,
    SideEffectSeqNumber,
    CreatedOn,
    FailureReason,
    FailureSeverity,
    FailureAckedUntil,
    SubjectVersion
FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SideEffect] (NOLOCK)
WHERE FailureSeverity IS NOT NULL