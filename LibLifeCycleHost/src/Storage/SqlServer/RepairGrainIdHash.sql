CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____RepairGrainIdHash] (
    @id                      NVARCHAR(80),
    @concurrencyToken        BINARY(8) OUTPUT,
    @offSyncConcurrencyToken BINARY(8) OUTPUT,
    @grainIdHash             INT
)
AS
    UPDATE [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
        SET [GrainIdHash] = @grainIdHash
        WHERE [Id] = @id AND ConcurrencyToken = @concurrencyToken;

    IF @@ROWCOUNT = 0
    BEGIN
        SET @concurrencyToken = NULL
        SELECT
            TOP 1 @offSyncConcurrencyToken = ConcurrencyToken
        FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
            WHERE Id = @id;
    END
    ELSE
    BEGIN
        SET @offSyncConcurrencyToken = NULL
        SELECT
            TOP 1 @concurrencyToken = ConcurrencyToken
        FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
            WHERE Id = @id;
    END
