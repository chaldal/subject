IF
    -- check if SysStart column included in index, if not then index will be (re)created for small _History tables
    -- large and huge tables need to be upgraded manually during scheduled maintenance
    (NOT EXISTS (
       select * from sys.indexes ix
       join sys.index_columns ic on ic.object_id = ix.object_id AND ic.index_id = ix.index_id
       join sys.columns c on c.object_id = ic.object_id AND c.column_id = ic.column_id
       WHERE
           ix.object_id = OBJECT_ID('[___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]')
           AND ix.name = 'IX____LIFECYCLE_NAME____History_Version'
           AND ic.is_included_column = 1 AND c.name = 'SysStart'))
   AND
       (SELECT SUM(p.rows)
         FROM sys.tables t
                  INNER JOIN sys.partitions p ON t.object_id = p.object_id
                  INNER JOIN sys.indexes i ON p.index_id = i.index_id AND p.object_id = i.object_id
         WHERE t.object_id = Object_id('[___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]') AND i.name = 'PK____LIFECYCLE_NAME____History')
            < 100000
BEGIN
    DROP INDEX IF EXISTS [IX____LIFECYCLE_NAME____History_Version] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
    CREATE NONCLUSTERED INDEX [IX____LIFECYCLE_NAME____History_Version] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] ([Version]) INCLUDE ([SysStart]);
END
