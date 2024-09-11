CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____InitializeState_V3] (
    @id                         NVARCHAR(80),
    @subjectTransactionId       UNIQUEIDENTIFIER,
    @subject                    VARBINARY(MAX),
    @operation                  VARBINARY(MAX),
    @lastOperationBy            NVARCHAR(80),
    @subjectLastUpdatedOn       DATETIMEOFFSET,
    @nextTickOn                 DATETIMEOFFSET,
    @nextTickFired              BIT,
    @nextTickContext            VARBINARY(MAX),
    @grainIdHash                INT,
    @nextSideEffectSeqNumber    BIGINT,
    @ourSubscriptions           VARBINARY(MAX),
    @creatorId                  NVARCHAR(80),
    @creatorLifeCycleName       NVARCHAR(80),
    @creatorSubscriptions       [___SCHEMA_NAME___].[SubscriptionNameAndLifeEventList] READONLY,
    @newSideEffects             [___SCHEMA_NAME___].[SideEffectList_V2] READONLY,
    @indices                    [___SCHEMA_NAME___].[IndexingList_V3]   READONLY,
    @promotedIndices            [___SCHEMA_NAME___].[PromotedIndexingList] READONLY,
    @callDedupActions           [___SCHEMA_NAME___].[CallDedupList_V2]  READONLY,
    @blobActions                [___SCHEMA_NAME___].[BlobActionList_V2] READONLY,
    @concurrencyToken           BINARY(8) OUTPUT,
    @offSyncConcurrencyToken    BINARY(8) OUTPUT,
    @version                    BIGINT OUTPUT,
    @violatedUniqueIndexKey     NVARCHAR(80) OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

    DECLARE @hasConcurrencyError BIT = 0

    IF @id IS NULL
        RAISERROR('___LIFECYCLE_NAME____InitializeState_V3 Param id should not be NULL', 11, 1);

    IF @nextSideEffectSeqNumber IS NULL AND ((SELECT COUNT(*) FROM @newSideEffects) > 0)
        RAISERROR('___LIFECYCLE_NAME____InitializeState_V3 Param nextSideEffectSeqNumber should not be NULL if there are any new side-effects', 11, 1);

    IF @grainIdHash IS NULL
        RAISERROR('___LIFECYCLE_NAME____InitializeState_V3 Param grainIdHash should not be NULL for insert', 11, 1);

    IF @subject IS NULL
        RAISERROR('___LIFECYCLE_NAME____InitializeState_V3 Param subject should not be NULL', 11, 1);
    IF @operation IS NULL
        RAISERROR('___LIFECYCLE_NAME____InitializeState_V3 Param operation should not be NULL', 11, 1);
    IF @lastOperationBy IS NULL
        RAISERROR('___LIFECYCLE_NAME____InitializeState_V3 Param lastOperationBy should not be NULL', 11, 1);
    IF @subjectLastUpdatedOn IS NULL
        RAISERROR('___LIFECYCLE_NAME____InitializeState_V3 Param subjectLastUpdatedOn should not be NULL', 11, 1);
    IF @ourSubscriptions IS NULL
        RAISERROR('___LIFECYCLE_NAME____InitializeState_V3 Param ourSubscriptions should not be NULL', 11, 1);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @hasConcurrencyError = 0 AND @subjectTransactionId IS NOT NULL
        BEGIN
            DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared]
            WHERE Id = @id AND SubjectTransactionId = @subjectTransactionId AND @concurrencyToken = PreparedInitializeConcurrencyToken;
            IF @@ROWCOUNT = 0
            BEGIN
                SET @concurrencyToken = NULL
                SET @version = NULL
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
                    SET @version = NULL
                    SET @hasConcurrencyError = 1
                    IF @@trancount > 0 ROLLBACK TRANSACTION;
                END
            END
        END

        IF @hasConcurrencyError = 0
        BEGIN
            BEGIN TRY
                DECLARE @insertStateOutputTable TABLE (ConcurrencyToken BINARY(8), Version BIGINT);
                INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
                    (Id, Subject, Operation, LastOperationBy, SubjectLastUpdatedOn,
                        NextTickOn, NextTickFired, NextTickContext, OurSubscriptions,
                        Version,
                        IsConstruction, NextSideEffectSeqNumber, GrainIdHash)
                OUTPUT INSERTED.ConcurrencyToken, INSERTED.Version INTO @insertStateOutputTable (ConcurrencyToken, Version)
                VALUES (
                    @id, @subject, @operation, @lastOperationBy, @subjectLastUpdatedOn,
                        @nextTickOn, @nextTickFired, @nextTickContext, @ourSubscriptions,
                        NEXT VALUE FOR [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Version],
                        1, ISNULL(@nextSideEffectSeqNumber, 0), @grainIdHash
                    );
                SELECT TOP 1 @concurrencyToken = ConcurrencyToken, @version = Version FROM @insertStateOutputTable;
            END TRY
            BEGIN CATCH
                DECLARE @errNumMainTable INT
                SET @errNumMainTable = ERROR_NUMBER()
                IF @errNumMainTable = 2627 -- PK constraint violation
                BEGIN
                    SET @concurrencyToken = NULL
                    SET @version = NULL
                    SET @hasConcurrencyError = 1
                    SELECT
                       TOP 1 @offSyncConcurrencyToken = ConcurrencyToken
                    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] WHERE Id = @id
                    IF @@trancount > 0 ROLLBACK TRANSACTION;
                END
                ELSE THROW;
            END CATCH
        END

        IF @hasConcurrencyError = 0 AND
            @creatorId IS NOT NULL AND @creatorLifeCycleName IS NOT NULL AND
            EXISTS (SELECT TOP 1 1 FROM @creatorSubscriptions)
        BEGIN
            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Subscription]
                (SubjectId, SubscriptionName, LifeCycleName, SubscriberId, LifeEvent)
                SELECT @id, SubscriptionName, @creatorLifeCycleName, @creatorId, LifeEvent FROM @creatorSubscriptions
        END

        ___UPSERT_STATE_SHARED___

        IF @@trancount > 0 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
    END CATCH
