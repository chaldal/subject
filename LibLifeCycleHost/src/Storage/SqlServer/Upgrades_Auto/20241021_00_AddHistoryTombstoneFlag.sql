IF NOT EXISTS
   (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
   WHERE COLUMN_NAME = 'Tombstone'
    AND TABLE_NAME = '___LIFECYCLE_NAME____History'
    AND TABLE_SCHEMA = '___SCHEMA_NAME___')
BEGIN
    ALTER TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
    ADD [Tombstone] BIT NULL;
END

IF (NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX____LIFECYCLE_NAME____History_TombstoneSysEnd'))
   AND
    -- will be applied automatically only to small _History tables, large and huge tables need to be upgraded manually during scheduled maintenance
    (SELECT SUM(p.rows)
        FROM sys.tables t
        INNER JOIN sys.partitions p ON t.object_id = p.object_id
        INNER JOIN sys.indexes i ON p.index_id = i.index_id AND p.object_id = i.object_id
        WHERE t.object_id = Object_id('[___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]') AND i.name = 'PK____LIFECYCLE_NAME____History')
        < 100000
BEGIN
    -- new column within same transaction requires dynamic SQL
    DECLARE @sql NVARCHAR(MAX) = 'CREATE NONCLUSTERED INDEX [IX____LIFECYCLE_NAME____History_TombstoneSysEnd] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] ([SysEnd]) WHERE [Tombstone] = 1'
    EXEC sp_executesql @sql
END

-- TODO: run this script to deduce tombstones in pre-existing data, won't be automated for faster migration
-- if your prod history data is too large then use AfterSubjectChange for specified period of time to flush legacy data w/o tombstones,
-- or - if it's safe - just truncate history table

-- UPDATE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
-- SET Tombstone = 1
--     FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
-- JOIN
-- (
--     SELECT Id, Max(Version) as Version FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] y
--     WHERE Id NOT IN (SELECT Id FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] x)
--     GROUP BY Id
-- ) ts ON ts.Id = [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History].Id and ts.Version = [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History].Version
