CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____RebuildIndices_V2] (
    @removeAllNonMatchingKeys   BIT,
    @indices                    [___SCHEMA_NAME___].[RebuildIndexList_V4] READONLY,
    @subjectUpdatedConcurrently BIT OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE indexTable
        FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index] AS indexTable
        INNER JOIN @indices input
            ON
                input.[Id] = indexTable.[SubjectId]
                AND (@removeAllNonMatchingKeys = 1 OR input.[Key] = indexTable.[Key]);

    INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index]
        ([SubjectId], [Key], [ValueStr], [ValueInt], [EnforceUniqueness])
            SELECT
                [Id],
                [Key],
                [ValueStr],
                [ValueInt],
                (CASE WHEN [Kind] IN (2,4) THEN 1 ELSE 0 END) AS [EnforceUniqueness]
                FROM @indices
                WHERE IsDelete = 0 AND [Kind] IN (1, 2, 3, 4) AND PromotedKey = '';

___REBUILD_SEARCH_INDICES_SQL___

___REBUILD_GEOGRAPHY_INDICES_SQL___

___REBUILD_ALL_PROMOTED_INDICES_SQL___

    -- concurrency check

    IF EXISTS
        (SELECT * FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] subject
         JOIN @indices input ON input.[Id] = subject.[Id]
         WHERE input.ConcurrencyToken <> subject.ConcurrencyToken)
    BEGIN
        ROLLBACK TRANSACTION
        SET @subjectUpdatedConcurrently = 1
    END
    ELSE
    BEGIN
        COMMIT TRANSACTION;
        SET @subjectUpdatedConcurrently = 0
    END

END TRY
BEGIN CATCH
IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
END CATCH