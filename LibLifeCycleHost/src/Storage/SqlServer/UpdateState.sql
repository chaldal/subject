CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____UpdateState_V4] (
    @id                         NVARCHAR(80),
    @subjectTransactionId       UNIQUEIDENTIFIER,
    @subject                    VARBINARY(MAX),
    @operation                  VARBINARY(MAX),
    @lastOperationBy            NVARCHAR(80),
    @subjectLastUpdatedOn       DATETIMEOFFSET,
    @nextTickOn                 DATETIMEOFFSET,
    @nextTickFired              BIT,
    @nextTickContext            VARBINARY(MAX),
    @nextSideEffectSeqNumber    BIGINT,
    @ourSubscriptions           VARBINARY(MAX),
    @skipHistory                BIT = 0,
    @newSideEffects             [___SCHEMA_NAME___].[SideEffectList_V2] READONLY,
    @indices                    [___SCHEMA_NAME___].[IndexingList_V3]   READONLY,
    @promotedIndices            [___SCHEMA_NAME___].[PromotedIndexingList] READONLY,
    @callDedupActions           [___SCHEMA_NAME___].[CallDedupList_V2]  READONLY,
    @blobActions                [___SCHEMA_NAME___].[BlobActionList_V2] READONLY,
    @subscriberLifeCycleName    NVARCHAR(80),
    @subscriberId               NVARCHAR(80),
    @subscriptionsToAdd         [___SCHEMA_NAME___].SubscriptionNameAndLifeEventList READONLY,
    @concurrencyToken           BINARY(8) OUTPUT,
    @offSyncConcurrencyToken    BINARY(8) OUTPUT,
    @version                    BIGINT OUTPUT,
    @violatedUniqueIndexKey     NVARCHAR(80) OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

    DECLARE @hasConcurrencyError BIT = 0

    IF @id IS NULL
        RAISERROR('___LIFECYCLE_NAME____UpdateState_V4 Param id should not be NULL', 11, 1);

    IF @nextSideEffectSeqNumber IS NULL AND ((SELECT COUNT(*) FROM @newSideEffects) > 0)
        RAISERROR('___LIFECYCLE_NAME____UpdateState_V4 Param nextSideEffectSeqNumber should not be NULL if there are any new side-effects', 11, 1);

    IF @concurrencyToken IS NULL
        RAISERROR('___LIFECYCLE_NAME____UpdateState_V4 Param concurrencyToken should not be NULL for existing state', 11, 1);

    IF @subject IS NULL
        RAISERROR('___LIFECYCLE_NAME____UpdateState_V4 Param subject should not be NULL', 11, 1);
    IF @operation IS NULL
        RAISERROR('___LIFECYCLE_NAME____UpdateState_V4 Param operation should not be NULL', 11, 1);
    IF @lastOperationBy IS NULL
        RAISERROR('___LIFECYCLE_NAME____UpdateState_V4 Param lastOperationBy should not be NULL', 11, 1);
    IF @subjectLastUpdatedOn IS NULL
        RAISERROR('___LIFECYCLE_NAME____UpdateState_V4 Param subjectLastUpdatedOn should not be NULL', 11, 1);
    IF @ourSubscriptions IS NULL
        RAISERROR('___LIFECYCLE_NAME____UpdateState_V4 Param ourSubscriptions should not be NULL', 11, 1);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @hasConcurrencyError = 0 AND @subjectTransactionId IS NOT NULL
        BEGIN
            DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared]
            WHERE Id = @id AND SubjectTransactionId = @subjectTransactionId;

            IF @@ROWCOUNT = 0
            BEGIN
                SET @concurrencyToken = NULL
                SET @version = NULL
                SET @hasConcurrencyError = 1
                SELECT
                    TOP 1 @offSyncConcurrencyToken = PreparedInitializeConcurrencyToken
                    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared]
                    WHERE Id = @id;
                IF @@trancount > 0 ROLLBACK TRANSACTION;
            END
        END

        IF @hasConcurrencyError = 0
        BEGIN
            -- Capture old values (to insert into HISTORY table)
            -- and new concurrency token to return back to application
            DECLARE @updateStateAndInsertHistoryOutputTable TABLE (
                [OldSubject]              VARBINARY(MAX)   NOT NULL,
                [OldOperation]            VARBINARY(MAX)   NOT NULL,
                [OldIsConstruction]       BIT              NOT NULL,
                [OldSubjectLastUpdatedOn] DATETIMEOFFSET   NOT NULL,
                [OldLastOperationBy]      NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS,
                [OldNextTickOn]           DATETIMEOFFSET,
                [OldNextTickFired]        BIT,
                [OldNextTickContext]      VARBINARY(MAX),
                [OldOurSubscriptions]     VARBINARY(MAX)   NOT NULL,
                [OldVersion]              BIGINT           NOT NULL,
                [OldSysStart]             DATETIMEOFFSET   NOT NULL,
                [NewSysStart]             DATETIMEOFFSET   NOT NULL,
                [ConcurrencyToken]        BINARY(8)        NOT NULL,
                [Version]                 BIGINT           NOT NULL,
                [OldNextSideEffectSeqNum] BIGINT           NOT NULL
            );

            UPDATE [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
                SET
                    Subject = @subject,
                    Operation = @operation,
                    LastOperationBy = @lastOperationBy,
                    IsConstruction = 0,
                    SubjectLastUpdatedOn = @subjectLastUpdatedOn,
                    NextTickOn = @nextTickOn,
                    NextTickFired = @nextTickFired,
                    NextTickContext = @nextTickContext,
                    OurSubscriptions = @ourSubscriptions,
                    Version = NEXT VALUE FOR [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Version],
                    SysStart = SYSDATETIMEOFFSET(),
                    NextSideEffectSeqNumber = ISNULL(@nextSideEffectSeqNumber, NextSideEffectSeqNumber)
                OUTPUT
                    DELETED.Subject, DELETED.Operation, DELETED.IsConstruction, DELETED.SubjectLastUpdatedOn, DELETED.LastOperationBy,
                    DELETED.NextTickOn, DELETED.NextTickFired, DELETED.NextTickContext, DELETED.OurSubscriptions, DELETED.Version,
                    DELETED.SysStart, INSERTED.SysStart, INSERTED.ConcurrencyToken, INSERTED.Version, DELETED.NextSideEffectSeqNumber
                INTO @updateStateAndInsertHistoryOutputTable (
                    OldSubject, OldOperation, OldIsConstruction, OldSubjectLastUpdatedOn, OldLastOperationBy,
                    OldNextTickOn, OldNextTickFired, OldNextTickContext, OldOurSubscriptions, OldVersion,
                    OldSysStart, NewSysStart, ConcurrencyToken, Version, OldNextSideEffectSeqNum)
                WHERE
                    Id = @id AND ConcurrencyToken = @concurrencyToken;

            IF @@ROWCOUNT = 0
            BEGIN
                SET @concurrencyToken = NULL
                SET @version = NULL
                SET @hasConcurrencyError = 1
                SELECT
                    TOP 1 @offSyncConcurrencyToken = ConcurrencyToken
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
                    WHERE Id = @id;
                IF @@trancount > 0 ROLLBACK TRANSACTION;
            END
            ELSE
            BEGIN
                IF @skipHistory = 0
                BEGIN
                    -- INSERT INTO History Table to maintain Audit Logs
                    INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
                        (Id, Subject, Operation, IsConstruction, SubjectLastUpdatedOn, LastOperationBy, NextTickOn, NextTickFired, NextTickContext, OurSubscriptions,
                            Version, NextSideEffectSeqNumber, SysStart, SysEnd)
                        SELECT @id, OldSubject, OldOperation, OldIsConstruction, OldSubjectLastUpdatedOn, OldLastOperationBy, OldNextTickOn, OldNextTickFired, OldNextTickContext, OldOurSubscriptions,
                            OldVersion, OldNextSideEffectSeqNum, OldSysStart, NewSysStart
                        FROM @updateStateAndInsertHistoryOutputTable;
                END

                SET @offSyncConcurrencyToken = NULL
                SELECT
                    TOP 1
                    @concurrencyToken = ConcurrencyToken, @version = Version
                FROM
                    @updateStateAndInsertHistoryOutputTable;
            END
        END

        IF (@hasConcurrencyError = 0 AND EXISTS (SELECT TOP 1 1 FROM @subscriptionsToAdd))
        BEGIN
            BEGIN TRY
                INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Subscription]
                (SubjectId, SubscriptionName, LifeCycleName, SubscriberId, LifeEvent)
                SELECT @id, SubscriptionName, @subscriberLifeCycleName, @subscriberId, LifeEvent FROM @subscriptionsToAdd

            END TRY
            BEGIN CATCH
                DECLARE @errNumSubscriptionTable INT
                SET @errNumSubscriptionTable = ERROR_NUMBER()
                IF @errNumSubscriptionTable = 2627 -- PK constraint violation
                BEGIN
                    SET @concurrencyToken = NULL
                    SET @hasConcurrencyError = 1
                    SELECT TOP 1 @offSyncConcurrencyToken = ConcurrencyToken
                    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
                    WHERE Id = @id;
                    IF @@trancount > 0 ROLLBACK TRANSACTION;
                END
                ELSE THROW;
            END CATCH
        END

        ___UPSERT_STATE_SHARED___

        IF @@trancount > 0 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
    END CATCH
