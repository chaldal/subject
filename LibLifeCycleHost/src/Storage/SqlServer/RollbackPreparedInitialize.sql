CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____RollbackPreparedInitialize_V2] (
    @id                         NVARCHAR(80),
    @subjectTransactionId       UNIQUEIDENTIFIER,
    @uniqueIndicesToRelease     [___SCHEMA_NAME___].[IndexingList_V3]   READONLY,
    @concurrencyToken           BINARY(8) OUTPUT,
    @offSyncConcurrencyToken    BINARY(8) OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

    DECLARE @hasConcurrencyError BIT = 0

    IF @id IS NULL
        RAISERROR('___LIFECYCLE_NAME____RollbackPreparedInitialize_V2 Param id should not be NULL', 11, 1);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @hasConcurrencyError = 0
        BEGIN
            DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared]
            WHERE Id = @id AND SubjectTransactionId = @subjectTransactionId AND @concurrencyToken = PreparedInitializeConcurrencyToken;
            IF @@ROWCOUNT = 0
            BEGIN
                SET @concurrencyToken = NULL
                SET @hasConcurrencyError = 1
                SELECT
                    TOP 1 @offSyncConcurrencyToken = PreparedInitializeConcurrencyToken
                    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared]
                    WHERE Id = @id;
                IF @offSyncConcurrencyToken IS NULL
                BEGIN
                    SELECT @offSyncConcurrencyToken = ConcurrencyToken FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] WHERE Id = @id
                END
                IF @@trancount > 0 ROLLBACK TRANSACTION;
            END
            ELSE
            BEGIN
                SELECT @offSyncConcurrencyToken = ConcurrencyToken FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] WHERE Id = @id
                IF @offSyncConcurrencyToken IS NOT NULL
                BEGIN
                    SET @concurrencyToken = NULL
                    SET @hasConcurrencyError = 1
                    IF @@trancount > 0 ROLLBACK TRANSACTION;
                END
            END
        END

        ___ROLLBACK_STATE_SHARED___

        IF @@trancount > 0 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
    END CATCH
