IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = 'DataProtectionKey' AND table_schema = '___SCHEMA_NAME___')
BEGIN
    CREATE TABLE [___SCHEMA_NAME___].[DataProtectionKey]
    (
	    [FriendlyName] NVARCHAR(100) NOT NULL PRIMARY KEY,
	    [Xml] XML MASKED WITH (FUNCTION = 'default()') NOT NULL
    )
END