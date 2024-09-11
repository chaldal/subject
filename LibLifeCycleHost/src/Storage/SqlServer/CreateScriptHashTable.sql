IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = '__ScriptHash' and table_schema = '___SCHEMA_NAME___')
BEGIN
    BEGIN TRANSACTION

    DECLARE @scriptHash NVARCHAR(MAX) =
        'CREATE TABLE [___SCHEMA_NAME___].[__ScriptHash] ([ScriptName] VARCHAR(200) NOT NULL PRIMARY KEY, Hash VARBINARY(8000))';
    EXEC sp_executesql @scriptHash WITH RESULT SETS NONE;

    COMMIT TRANSACTION
END