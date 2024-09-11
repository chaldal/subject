CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SetTickStateAndSubscriptions_V2] (
    @id                         NVARCHAR(80),
    @nextTickOn                 DATETIMEOFFSET,
    @nextTickFired              BIT,
    @nextTickContext            VARBINARY(MAX),
    @nextSideEffectSeqNumber    BIGINT,
    @ourSubscriptions           VARBINARY(MAX),
    @newSideEffects             [___SCHEMA_NAME___].[SideEffectList_V2] READONLY,
    @concurrencyToken           BINARY(8) OUTPUT,
    @offSyncConcurrencyToken    BINARY(8) OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

    DECLARE @hasConcurrencyError BIT = 0
    DECLARE @version BIGINT = NULL

    IF @id IS NULL
        RAISERROR('___LIFECYCLE_NAME____SetTickStateAndSubscriptions_V2 Param id should not be NULL', 11, 1);

    IF @nextSideEffectSeqNumber IS NULL AND ((SELECT COUNT(*) FROM @newSideEffects) > 0)
        RAISERROR('___LIFECYCLE_NAME____SetTickStateAndSubscriptions_V2 Param nextSideEffectSeqNumber should not be NULL if there are any new side-effects', 11, 1);

    IF @concurrencyToken IS NULL
        RAISERROR('___LIFECYCLE_NAME____SetTickStateAndSubscriptions_V2 Param concurrencyToken should not be NULL for existing state', 11, 1);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @hasConcurrencyError = 0
        BEGIN
            DECLARE @setNextTickAndSubscriptionsOutputTable TABLE (
                ConcurrencyToken BINARY(8) NOT NULL,
                [Version] BIGINT NOT NULL
            );
            -- NOTE: Consciously deciding to NOT archive the current row in the HISTORY table
            -- as thats primarily meant for audit-tracking actions, not timers
            UPDATE [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
            SET
                NextTickOn = @nextTickOn, NextTickFired = @nextTickFired, NextTickContext = @nextTickContext, NextSideEffectSeqNumber = ISNULL(@nextSideEffectSeqNumber, NextSideEffectSeqNumber),
                OurSubscriptions = @ourSubscriptions
            OUTPUT
                INSERTED.ConcurrencyToken, INSERTED.[Version]
                INTO
                @setNextTickAndSubscriptionsOutputTable (ConcurrencyToken, Version)
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
                    @setNextTickAndSubscriptionsOutputTable;
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
