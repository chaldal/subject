IF
    -- WARNING: don't check for existing index like this !
    -- it also must check for schema name (e.g. if many ecosystems hosted in one db)
    -- keep this migration intact for historical purposes.
    (NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX____LIFECYCLE_NAME____History_Version'))
    AND
    -- will be applied automatically only to small _History tables, large and huge tables need to be upgraded manually during scheduled maintenance
    (SELECT SUM(p.rows)
        FROM sys.tables t
        INNER JOIN sys.partitions p ON t.object_id = p.object_id
        INNER JOIN sys.indexes i ON p.index_id = i.index_id AND p.object_id = i.object_id
        WHERE t.object_id = Object_id('[___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]') AND i.name = 'PK____LIFECYCLE_NAME____History')
        < 10000
BEGIN
    CREATE INDEX IX____LIFECYCLE_NAME____History_Version ON [___SCHEMA_NAME___].___LIFECYCLE_NAME____History ([Version])
END
