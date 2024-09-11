CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____ClearExpiredHistoryBatch] (
    -- currently supported modes:
    -- 1 AfterSubjectDeletion - will delete only history rows under expired tombstones
    -- 2 AfterSubjectChange - will delete history rows regardless of tombstones
    @mode INT,
    @expireAfter DATETIMEOFFSET,
    @batchSize INT
)
AS
    SET NOCOUNT OFF;

    DECLARE @Id NVARCHAR(80), @Version BIGINT;
    DECLARE @RowsDeleted INT = 0

    CREATE TABLE #HistoryPkToDelete (
        Id NVARCHAR(80) COLLATE Latin1_General_CS_AS_KS_WS,
        Version BIGINT
    );

    IF @mode = 1
    BEGIN
        -- find history beneath eligible tombstones (check only oldest tombstones for speed)
        INSERT INTO #HistoryPkToDelete (Id, Version)
        SELECT TOP (@batchSize) h.Id, h.Version
        FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] h
        JOIN (
            SELECT TOP (@batchSize) Id, Version
            FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
            WHERE [Tombstone] = 1 AND SysEnd < @expireAfter
            ORDER BY [SysEnd]) t ON h.Id = t.Id AND h.Version < t.Version

        -- find eligible tombstones without any remaining history beneath (check only oldest tombstones for speed)
        INSERT INTO #HistoryPkToDelete (Id, Version)
        SELECT h.Id, h.Version FROM
        (
            SELECT TOP (@batchSize) Id, Version
            FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] (NOLOCK)
            WHERE Tombstone = 1
            ORDER BY [SysEnd]
        ) h
        WHERE NOT EXISTS (SELECT * FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] h2 WHERE h.Id = h2.Id and h2.Version < h.Version)
    END
    ELSE IF @mode = 2
    BEGIN
        -- piggyback on Version index, it correlates with SysStart - CAUTION: it relies on Version counter being global
        INSERT INTO #HistoryPkToDelete (Id, Version)
        SELECT Id, Version FROM (
            SELECT TOP (@batchSize) h.Id, h.Version, h.SysStart
            FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] (NOLOCK) h
            ORDER BY [Version]) t
            WHERE SysStart < @expireAfter
    END
    ELSE
    BEGIN
        DROP TABLE #HistoryPkToDelete
        RAISERROR('___LIFECYCLE_NAME____ClearExpiredHistoryBatch mode is not supported', 11, 1);
    END

    SET NOCOUNT ON;

    -- mass delete detected rows, works well for larger @batchSize, but might be susceptible to lock escalation
    DELETE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
    JOIN #HistoryPkToDelete x ON x.Id = [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History].Id and x.Version = [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History].Version

    SELECT @RowsDeleted = @@ROWCOUNT

    -- ANOTHER OPTION: delete rows one-by-one by primary key to avoid lock escalation? Works poorly even for moderate @batchSize
    /*
    DECLARE cur1 CURSOR LOCAL FOR SELECT Id, Version FROM #HistoryPkToDelete;
    OPEN cur1;
    FETCH NEXT FROM cur1 INTO @Id, @Version;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
        WHERE Id = @Id AND Version = @Version;
        SET @RowsDeleted = @RowsDeleted + @@ROWCOUNT;
        FETCH NEXT FROM cur1 INTO @Id, @Version;
    END
    CLOSE cur1;
    DEALLOCATE cur1;
    */

    DROP TABLE #HistoryPkToDelete

    SELECT @RowsDeleted AS RowsDeleted

