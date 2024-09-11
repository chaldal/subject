DECLARE @schemaInitialized CHAR(1)
IF EXISTS (SELECT * FROM information_schema.tables where table_schema = '___SCHEMA_NAME___' and table_name = '_Meta')
    -- some subject tables already exist i.e. schema already initialized, will try to apply auto upgrades on next step
    SET @schemaInitialized = '1'
ELSE
    -- brand new schema w/o subject tables, no need to apply auto upgrades, will create in latest format on next step
    SET @schemaInitialized = '0'

IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = '__SchemaUpgrade' and table_schema = '___SCHEMA_NAME___')
BEGIN
    -- schema already exist but not __SchemaUpgrade
    BEGIN TRY
        SET XACT_ABORT ON;
        SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; -- avoid racy insert
        BEGIN TRANSACTION

        DECLARE @schemaUpgrade NVARCHAR(MAX) =
            'CREATE TABLE [___SCHEMA_NAME___].[__SchemaUpgrade] ([SchemaInitialized] BIT, [LastUpgradeScriptName] VARCHAR(200))';
        EXEC sp_executesql @schemaUpgrade WITH RESULT SETS NONE;

        DECLARE @insertSchemaUpgrade_New NVARCHAR(MAX) =
            'IF NOT EXISTS (SELECT * FROM [___SCHEMA_NAME___].[__SchemaUpgrade])
                INSERT [___SCHEMA_NAME___].[__SchemaUpgrade] ([SchemaInitialized], [LastUpgradeScriptName]) VALUES (' + @schemaInitialized + ', '''')'
        EXEC sp_executesql @insertSchemaUpgrade_New WITH RESULT SETS NONE;

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
    END CATCH
END

ELSE -- some deployed databases were affected by bug that created __SchemaUpgrade table but did not insert the row, remediate

BEGIN
    BEGIN TRY
        SET XACT_ABORT ON;
        SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; -- avoid racy insert
        DECLARE @insertSchemaUpgrade_Existing NVARCHAR(MAX) =
            'IF NOT EXISTS (SELECT * FROM [___SCHEMA_NAME___].[__SchemaUpgrade])
                INSERT [___SCHEMA_NAME___].[__SchemaUpgrade] ([SchemaInitialized], [LastUpgradeScriptName]) VALUES (' + @schemaInitialized + ', '''')'
        EXEC sp_executesql @insertSchemaUpgrade_Existing WITH RESULT SETS NONE;
    END TRY
    BEGIN CATCH
    IF @@trancount > 0 ROLLBACK TRANSACTION;
            ;THROW;
    END CATCH
END