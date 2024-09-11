IF NOT EXISTS (
    SELECT
        schema_name
    FROM
        information_schema.schemata
    WHERE
        schema_name = '___SCHEMA_NAME___' )
BEGIN
    DECLARE @schemaSql NVARCHAR(MAX) = 'CREATE SCHEMA [___SCHEMA_NAME___]';
    EXEC sp_executesql @schemaSql WITH RESULT SETS NONE;
END