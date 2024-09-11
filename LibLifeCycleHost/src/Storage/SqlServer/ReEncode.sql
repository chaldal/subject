CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____ReEncode] (
    @data [___SCHEMA_NAME___].[ReEncodeSubjectsList] READONLY,
    @subscriptions [___SCHEMA_NAME___].[ReEncodeSubscriptionsList] READONLY,
    @subjectUpdatedConcurrently BIT OUTPUT
)
AS
    SET XACT_ABORT, NOCOUNT ON;

BEGIN TRY
BEGIN TRANSACTION;

    UPDATE s
    SET [LifeEvent] = input.LifeEvent
    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Subscription] s
    JOIN @subscriptions input ON
        input.[SubjectId] = s.[SubjectId]
        AND input.[SubscriptionName] = s.[SubscriptionName]
        AND input.[LifeCycleName] = s.[LifeCycleName]
        AND input.[SubscriberId] = s.[SubscriberId]

    UPDATE s
    SET
        [Subject] = ISNULL(input.Subject, s.Subject),
        [Operation] = ISNULL(input.Operation, s.Operation),
        [OurSubscriptions] = ISNULL(input.OurSubscriptions, s.OurSubscriptions)
    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] s
    JOIN @data input ON input.Id = s.Id AND input.ConcurrencyToken = s.ConcurrencyToken

    IF @@ROWCOUNT < (SELECT COUNT(*) FROM @data)
    BEGIN
        ROLLBACK TRANSACTION
        SET @subjectUpdatedConcurrently = 1
    END
    ELSE
    BEGIN
        COMMIT TRANSACTION;
        SET @subjectUpdatedConcurrently = 0
    END

END TRY
BEGIN CATCH
IF @@trancount > 0 ROLLBACK TRANSACTION;
        ;THROW;
END CATCH