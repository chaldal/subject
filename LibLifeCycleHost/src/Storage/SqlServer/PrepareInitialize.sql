CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____PrepareInitialize_V2] (
    @id                         NVARCHAR(80),
    @preparedTransactionalState VARBINARY(MAX),
    @subjectTransactionId       UNIQUEIDENTIFIER,
    @uniqueIndicesToReserve     [___SCHEMA_NAME___].[IndexingList_V3] READONLY,
    @concurrencyToken           BINARY(8) OUTPUT,
    @offSyncConcurrencyToken    BINARY(8) OUTPUT,
    @violatedUniqueIndexKey     NVARCHAR(80) OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

    DECLARE @hasConcurrencyError BIT = 0

    IF @id IS NULL
        RAISERROR('___LIFECYCLE_NAME____PrepareInitialize_V2 Param id should not be NULL', 11, 1);

    IF @preparedTransactionalState IS NULL
        RAISERROR('___LIFECYCLE_NAME____PrepareInitialize_V2 Param preparedTransactionalState should not be NULL', 11, 1);
    IF @subjectTransactionId IS NULL
        RAISERROR('___LIFECYCLE_NAME____PrepareInitialize_V2 Param subjectTransactionId should not be NULL', 11, 1);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @hasConcurrencyError = 0
        BEGIN
            BEGIN TRY
                DECLARE @insertPreparedOutputTable TABLE (PreparedInitializeConcurrencyToken BINARY(8));
                INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared]
                    (Id, PreparedTransactionalState, SubjectTransactionId, SysStart)
                OUTPUT INSERTED.PreparedInitializeConcurrencyToken INTO @insertPreparedOutputTable (PreparedInitializeConcurrencyToken)
                VALUES (@id, @preparedTransactionalState, @subjectTransactionId, SYSDATETIMEOFFSET());

                SELECT @offSyncConcurrencyToken = ConcurrencyToken FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] WHERE Id = @id
                IF @offSyncConcurrencyToken IS NOT NULL
                BEGIN
                    SET @concurrencyToken = NULL
                    SET @hasConcurrencyError = 1
                    IF @@trancount > 0 ROLLBACK TRANSACTION;
                END
                ELSE
                BEGIN
                    SELECT TOP 1 @concurrencyToken = PreparedInitializeConcurrencyToken FROM @insertPreparedOutputTable;
                END
            END TRY
            BEGIN CATCH
                DECLARE @errNumPreparedTable INT
                SET @errNumPreparedTable = ERROR_NUMBER()
                IF @errNumPreparedTable = 2627 -- PK constraint violation
                BEGIN
                    SET @concurrencyToken = NULL
                    SET @hasConcurrencyError = 1
                    SELECT
                        TOP 1 @offSyncConcurrencyToken = PreparedInitializeConcurrencyToken
                    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared] WHERE Id = @id
                    IF @@trancount > 0 ROLLBACK TRANSACTION;
                END
                ELSE THROW;
            END CATCH
        END

        ___PREPARE_STATE_SHARED___

        IF @@trancount > 0 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
    END CATCH
