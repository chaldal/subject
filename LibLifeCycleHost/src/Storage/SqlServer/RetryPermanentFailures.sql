CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____RetryPermanentFailures_V2] (
    @id                         NVARCHAR(80),
    @nextSideEffectSeqNumber    BIGINT,
    @newSideEffects             [___SCHEMA_NAME___].[SideEffectList_V2] READONLY,
    @failedSideEffectsToDelete  [___SCHEMA_NAME___].[GuidList] READONLY,
    @concurrencyToken           BINARY(8) OUTPUT,
    @offSyncConcurrencyToken    BINARY(8) OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

    DECLARE @hasConcurrencyError BIT = 0
    DECLARE @version BIGINT = NULL

    IF @id IS NULL
        RAISERROR('___LIFECYCLE_NAME____RetryPermanentFailures_V2 Param id should not be NULL', 11, 1);

    IF @nextSideEffectSeqNumber IS NULL
        RAISERROR('___LIFECYCLE_NAME____RetryPermanentFailures_V2 Param nextSideEffectSeqNumber should not be NULL', 11, 1);

    IF @concurrencyToken IS NULL
        RAISERROR('___LIFECYCLE_NAME____RetryPermanentFailures_V2 Param concurrencyToken should not be NULL for existing state', 11, 1);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @hasConcurrencyError = 0 AND EXISTS (SELECT TOP 1 1 FROM @failedSideEffectsToDelete)
        BEGIN
            DELETE sideEffectTable
            FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SideEffect] AS sideEffectTable
            JOIN @failedSideEffectsToDelete input
                ON
                    sideEffectTable.SubjectId = @id AND
                    sideEffectTable.SideEffectId = input.[Id]

            DECLARE @setNextSideEffectSeqNumberOutputTable TABLE (
                ConcurrencyToken BINARY(8) NOT NULL,
                [Version] BIGINT NOT NULL
            );
            UPDATE [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
            SET
                NextSideEffectSeqNumber = ISNULL(@nextSideEffectSeqNumber, NextSideEffectSeqNumber)
                OUTPUT
                INSERTED.ConcurrencyToken, INSERTED.[Version]
            INTO
                @setNextSideEffectSeqNumberOutputTable (ConcurrencyToken, [Version])
            WHERE
                Id = @id AND ConcurrencyToken = @concurrencyToken;
            IF @@ROWCOUNT = 0
            BEGIN
                SET @concurrencyToken = NULL
                SET @hasConcurrencyError = 1
                SELECT
                    TOP 1 @offSyncConcurrencyToken = ConcurrencyToken
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
                WHERE Id = @id;
                IF @@trancount > 0 ROLLBACK TRANSACTION;
            END
            ELSE
            BEGIN
                SET @offSyncConcurrencyToken = NULL
                SELECT
                    TOP 1 @concurrencyToken = ConcurrencyToken, @version = [Version]
                FROM
                    @setNextSideEffectSeqNumberOutputTable;
            END
        END

        IF @hasConcurrencyError = 0
        BEGIN
            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SideEffect]
                (SubjectId, SideEffectId, SideEffectTarget, SideEffect, SideEffectSeqNumber, SubjectVersion)
                SELECT @id, Id, SideEffectTarget, SideEffect, SideEffectSeqNumber, @version
                    FROM @newSideEffects;
        END

        IF @@trancount > 0 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
    END CATCH
