SELECT
    '[___LIFECYCLE_NAME___]' as Subj,
    [___SCHEMA_NAME___].decode(PreparedTransactionalState) AS PreparedTransactionalState,
    Id,
    SubjectTransactionId,
    SysStart
FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared] (NOLOCK)
WHERE DATEDIFF(MINUTE, [SysStart], SYSDATETIMEOFFSET()) > 15