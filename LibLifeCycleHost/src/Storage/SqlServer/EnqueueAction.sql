CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____EnqueueAction_V2] (
    @id                         NVARCHAR(80),
    @now                        DATETIMEOFFSET,
    @nextSideEffectSeqNumber    BIGINT,
    @newSideEffects             [___SCHEMA_NAME___].[SideEffectList_V2] READONLY,
    @callDedupActions           [___SCHEMA_NAME___].[CallDedupList_V2]  READONLY,
    @concurrencyToken           BINARY(8) OUTPUT,
    @offSyncConcurrencyToken    BINARY(8) OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

    DECLARE @hasConcurrencyError BIT = 0
    DECLARE @version BIGINT = NULL

    IF @id IS NULL
        RAISERROR('___LIFECYCLE_NAME____EnqueueAction_V2 Param id should not be NULL', 11, 1);

    IF @nextSideEffectSeqNumber IS NULL AND ((SELECT COUNT(*) FROM @newSideEffects) > 0)
        RAISERROR('___LIFECYCLE_NAME____EnqueueAction_V2 Param nextSideEffectSeqNumber should not be NULL if there are any new side-effects', 11, 1);

    IF @concurrencyToken IS NULL
        RAISERROR('___LIFECYCLE_NAME____EnqueueAction_V2 Param concurrencyToken should not be NULL for existing state', 11, 1);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @hasConcurrencyError = 0
        BEGIN
            DECLARE @enqueueActionOutputTable TABLE (
                ConcurrencyToken BINARY(8) NOT NULL,
                [Version] BIGINT NOT NULL
            );
            -- NOTE: Consciously deciding to NOT archive the current row in the HISTORY table
            -- as thats primarily meant for audit-tracking actions, not timers
            UPDATE [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
                SET
                    NextSideEffectSeqNumber = ISNULL(@nextSideEffectSeqNumber, NextSideEffectSeqNumber)
                OUTPUT
                    INSERTED.ConcurrencyToken, INSERTED.[Version]
                INTO
                    @enqueueActionOutputTable (ConcurrencyToken, Version)
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
                    TOP 1
                    @concurrencyToken = ConcurrencyToken,
                    @version = [Version]
                FROM
                    @enqueueActionOutputTable;
            END
        END

        -- Update SideEffects assuming we have no concurrency issues
        IF @hasConcurrencyError = 0
        BEGIN
            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SideEffect]
                (SubjectId, SideEffectId, SideEffectTarget, SideEffect, SideEffectSeqNumber, SubjectVersion)
                SELECT @id, Id, SideEffectTarget, SideEffect, SideEffectSeqNumber, @version
                    FROM @newSideEffects;

            -- Transactional delete of the side effect which led to this Enqueue (think Continue or Compensate in response handler)
            -- to eliminate partial failure that can lead to unnecessary SE retrial
            -- Hack to avoid breaking change: piggybacking on the fact that the SideEffectId is the same as CallId in Dedup table
            DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SideEffect]
            WHERE SubjectId = @id AND SideEffectId IN (SELECT [CallId] FROM @callDedupActions WHERE [IsDelete] = 0 AND [IsInsert] = 1)
        END

        IF (@hasConcurrencyError = 0 AND
           EXISTS (SELECT TOP 1 1 FROM @callDedupActions))
        BEGIN
            -- delete
            DELETE dedupTable
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____CallDedup] AS dedupTable
                INNER JOIN @callDedupActions input
                    ON
                        input.[IsDelete]            = 1 AND
                        input.[IsInsert]            = 0 AND
                        input.[CallerLifeCycleName] = dedupTable.[CallerLifeCycleName] AND
                        input.[CallerId]            = dedupTable.[CallerId] AND
                        dedupTable.[SubjectId]      = @id;
            -- insert
            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____CallDedup]
                ([SubjectId], [CallerLifeCycleName], [CallerId], [CallId], [CalledOn])
                    SELECT @id, [CallerLifeCycleName], [CallerId], [CallId], @now FROM
                        @callDedupActions WHERE [IsDelete] = 0 AND [IsInsert] = 1
            -- update
            UPDATE dedupTable
                SET
                    dedupTable.[CallId] = input.[CallId],
                    dedupTable.[CalledOn] = @now
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____CallDedup] AS dedupTable
                INNER JOIN @callDedupActions input
                    ON
                        input.[IsDelete]            = 0 AND
                        input.[IsInsert]            = 0 AND
                        input.[CallerLifeCycleName] = dedupTable.[CallerLifeCycleName] AND
                        input.[CallerId]            = dedupTable.[CallerId] AND
                        dedupTable.[SubjectId]      = @id;
        END

        IF @@trancount > 0 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
    END CATCH
