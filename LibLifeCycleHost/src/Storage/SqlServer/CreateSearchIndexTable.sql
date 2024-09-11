IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = '___LIFECYCLE_NAME____SearchIndex' AND table_schema = '___SCHEMA_NAME___')
BEGIN
    DECLARE @searchIndexTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SearchIndex]
        (
            [Id]                NVARCHAR(160) COLLATE Latin1_General_CS_AS_KS_WS NOT NULL CONSTRAINT PK____LIFECYCLE_NAME____SearchIndex PRIMARY KEY,
            [SubjectId]         NVARCHAR(80)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [Key]               NVARCHAR(80)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [ValueStr]          NVARCHAR(MAX) COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            CONSTRAINT CK____LIFECYCLE_NAME____Id_IsConsistent CHECK([Id] = [SubjectId] + ''_'' + [Key])
        );';

    DECLARE @searchIndexTableIndices NVARCHAR(MAX) = '
        CREATE FULLTEXT INDEX ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SearchIndex](ValueStr)
        KEY INDEX PK____LIFECYCLE_NAME____SearchIndex
        ON [SubjectSearchCatalog]
        WITH STOPLIST = SYSTEM;';

    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
        EXEC sp_executesql @searchIndexTableSql   WITH RESULT SETS NONE;
    COMMIT;

    EXEC sp_executesql @searchIndexTableIndices WITH RESULT SETS NONE;
END;

