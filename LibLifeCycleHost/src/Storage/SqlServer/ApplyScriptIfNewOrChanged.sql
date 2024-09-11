CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[__ApplyScriptIfNewOrChanged]
    @scriptName VARCHAR(200),
    @sql AS NVARCHAR(MAX),
    @hash AS VARBINARY(8000)
AS
    IF NOT EXISTS (SELECT * FROM [___SCHEMA_NAME___].__ScriptHash WHERE ScriptName = @scriptName AND [Hash] = @hash)
    BEGIN
        EXEC sp_executesql @sql

        IF NOT EXISTS (SELECT * FROM [___SCHEMA_NAME___].__ScriptHash WHERE ScriptName = @scriptName)
        BEGIN
            BEGIN TRY
                INSERT [___SCHEMA_NAME___].__ScriptHash (ScriptName, [Hash]) VALUES (@scriptName, @hash)
            END TRY
            BEGIN CATCH
                DECLARE @errNumMainTable INT
                SET @errNumMainTable = ERROR_NUMBER()
                IF @errNumMainTable <> 2627 THROW; -- tolerate PK constraint violation e.g. multiple silos starting
            END CATCH
        END
        ELSE
        BEGIN
            UPDATE [___SCHEMA_NAME___].__ScriptHash SET [Hash] = @hash WHERE ScriptName = @scriptName
        END
    END