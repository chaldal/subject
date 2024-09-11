CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[_SequenceCurrentValue] (@sequenceName  NVARCHAR(100))
AS
BEGIN
    SELECT TOP 1 last_used_value
    FROM sys.sequences se
    JOIN sys.schemas sc on se.schema_id = sc.schema_id
    WHERE sc.name = '___SCHEMA_NAME___' AND se.name = '_Sequence_' + @sequenceName
END
