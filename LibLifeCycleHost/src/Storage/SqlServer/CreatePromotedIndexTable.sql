DECLARE @tableName NVARCHAR(MAX) = '___LIFECYCLE_NAME____Index____INDEX_NAME___'

IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = @tableName AND table_schema = '___SCHEMA_NAME___' )
BEGIN
    DECLARE @promotedIndexTableSql NVARCHAR(MAX) = '
    CREATE TABLE [___SCHEMA_NAME___].[' + @tableName + ']
    (
        [SubjectId]         NVARCHAR(80)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
        [PromotedValue]     NVARCHAR(100) COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
        [Key]               NVARCHAR(80)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
        [ValueStr]          NVARCHAR(500) COLLATE Latin1_General_CS_AS_KS_WS NULL,
        [ValueInt]          BIGINT NULL
    );';

    DECLARE @indices NVARCHAR(MAX) = '
        CREATE CLUSTERED INDEX [CX_' + @tableName + '] ON [___SCHEMA_NAME___].[' + @tableName + '] ([SubjectId], [Key]);
        CREATE NONCLUSTERED INDEX [IX_' + @tableName + '_PromotedKeyValues] ON [___SCHEMA_NAME___].[' + @tableName + '] ([PromotedValue], [Key], [ValueStr], [ValueInt] DESC);'

    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    EXEC sp_executesql @promotedIndexTableSql WITH RESULT SETS NONE;
    EXEC sp_executesql @indices               WITH RESULT SETS NONE;
    COMMIT;

END;
