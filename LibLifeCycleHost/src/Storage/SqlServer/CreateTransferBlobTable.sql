IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = 'TransferBlob' AND table_schema = '___SCHEMA_NAME___' )
BEGIN
    DECLARE @tableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].TransferBlob
        (
            [Id] UNIQUEIDENTIFIER PRIMARY KEY,
            [Bytes] VARBINARY(MAX),
            [CreatedOn] DATETIMEOFFSET NOT NULL
        );';

    EXEC sp_executesql @tableSql WITH RESULT SETS NONE;
END;