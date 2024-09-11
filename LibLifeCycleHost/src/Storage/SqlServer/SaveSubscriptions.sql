CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SaveSubscriptions] (
    @id                      NVARCHAR(80),
    @lifeCycleName           NVARCHAR(80),
    @subscriberId            NVARCHAR(80),
    @subscriptionsToAdd      [___SCHEMA_NAME___].SubscriptionNameAndLifeEventList READONLY,
    @subscriptionsToRemove   [___SCHEMA_NAME___].SubscriptionNameList READONLY,
    @concurrencyToken        BINARY(8) OUTPUT,
    @offSyncConcurrencyToken BINARY(8) OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

    DECLARE @hasConcurrencyError BIT = 0

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @hasConcurrencyError = 0
        BEGIN
            BEGIN TRY
                DELETE s FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Subscription] s
                JOIN @subscriptionsToRemove i
                ON s.SubjectId = @id AND s.LifeCycleName = @lifeCycleName AND s.SubscriberId = @subscriberId AND s.SubscriptionName = i.SubscriptionName;

                INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Subscription]
                (SubjectId, SubscriptionName, LifeCycleName, SubscriberId, LifeEvent)
                SELECT @id, SubscriptionName, @lifeCycleName, @subscriberId, LifeEvent FROM @subscriptionsToAdd

            END TRY
            BEGIN CATCH
                DECLARE @errNumSubscriptionTable INT
                SET @errNumSubscriptionTable = ERROR_NUMBER()
                IF @errNumSubscriptionTable = 2627 -- PK constraint violation
                BEGIN
                    SET @concurrencyToken = NULL
                    SET @hasConcurrencyError = 1
                    SELECT
                        TOP 1 @offSyncConcurrencyToken = ConcurrencyToken
                    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
                        WHERE Id = @id;
                    IF @@trancount > 0 ROLLBACK TRANSACTION;
                END
                ELSE THROW;
            END CATCH
        END

        IF @hasConcurrencyError = 0
        BEGIN
            DECLARE @dummyUpdateStateOutputTable TABLE (
                ConcurrencyToken BINARY(8) NOT NULL
            );
            UPDATE [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
                SET
                    NextTickFired = NextTickFired
                OUTPUT
                    INSERTED.ConcurrencyToken
                INTO
                    @dummyUpdateStateOutputTable (ConcurrencyToken)
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
                    TOP 1 @concurrencyToken = ConcurrencyToken
                FROM
                    @dummyUpdateStateOutputTable;
            END
        END

        IF @@trancount > 0 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
    END CATCH