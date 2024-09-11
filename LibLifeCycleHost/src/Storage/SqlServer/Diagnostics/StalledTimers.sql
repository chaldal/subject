SELECT
    '[___LIFECYCLE_NAME___]' as Subj,
    Id,
    NextTickOn,
    NextTickFired,
    SubjectLastUpdatedOn,
    [___SCHEMA_NAME___].decode(NextTickContext) AS NextTickContextDecoded
FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] (NOLOCK)
WHERE
    NOT EXISTS (SELECT * FROM [___SCHEMA_NAME___].[_Meta_Index] (NOLOCK) WHERE [SubjectId] = '___LIFECYCLE_NAME___' AND [Key] = 'RebuildingTimersAndSubs')
    AND [NextTickOn] IS NOT NULL
    AND [NextTickFired] = 0
    AND DATEDIFF(MINUTE, [NextTickOn], SYSDATETIMEOFFSET()) > 5
    AND DATEDIFF(MINUTE, [SubjectLastUpdatedOn], SYSDATETIMEOFFSET()) > 3