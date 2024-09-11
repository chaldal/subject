CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____ClearState] (
	@id                      NVARCHAR(100),
	@concurrencyToken        BINARY(8),
	@skipHistory             BIT = 0,
	@offSyncConcurrencyToken BINARY(8) OUTPUT,
	@isDeleted               BIT OUTPUT,
	@sideEffectId            UNIQUEIDENTIFIER = NULL
)
AS
	SET XACT_ABORT, NOCOUNT ON;
	-- Parameter validation
	IF @id IS NULL
		RAISERROR('ClearState Param id should not be NULL', 11, 1);
	IF @concurrencyToken IS NULL
		RAISERROR('ClearState Param concurrencyToken should not be NULL', 11, 1);

	BEGIN TRY
		BEGIN TRANSACTION;
			-- Capture old values (to insert into HISTORY table)
			-- and new concurrency token to return back to application
			DECLARE @outputTable TABLE (
				[OldSubject]              VARBINARY(MAX)   NOT NULL,
				[OldOperation]            VARBINARY(MAX)   NOT NULL,
				[OldIsConstruction]       BIT              NOT NULL,
				[OldSubjectLastUpdatedOn] DATETIMEOFFSET   NOT NULL,
				[OldLastOperationBy]      NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
				[OldNextTickOn]           DATETIMEOFFSET,
				[OldNextTickFired]        BIT,
				[OldNextTickContext]      VARBINARY(MAX),
				[OldOurSubscriptions]     VARBINARY(MAX)   NOT NULL,
				[OldVersion]              BIGINT           NOT NULL,
				[OldSysStart]             DATETIMEOFFSET   NOT NULL,
				[OldNextSideEffectSeqNum] BIGINT           NOT NULL
			);

			DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
				OUTPUT
					DELETED.Subject, DELETED.Operation, DELETED.IsConstruction, DELETED.SubjectLastUpdatedOn,
					DELETED.LastOperationBy, DELETED.NextTickOn, DELETED.NextTickFired, DELETED.NextTickContext, DELETED.OurSubscriptions, DELETED.Version,
					DELETED.SysStart, DELETED.NextSideEffectSeqNumber
				INTO @outputTable (
					OldSubject, OldOperation, OldIsConstruction, OldSubjectLastUpdatedOn, OldLastOperationBy,
					OldNextTickOn, OldNextTickFired, OldNextTickContext, OldOurSubscriptions, OldVersion, OldSysStart, OldNextSideEffectSeqNum)
				WHERE
					Id = @id AND ConcurrencyToken = @concurrencyToken;

			IF @@ROWCOUNT = 0
			BEGIN
				SET @isDeleted = 0
				SELECT
					TOP 1 @offSyncConcurrencyToken = ConcurrencyToken
				FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
					WHERE Id = @id;
			END
			ELSE
			BEGIN
				IF @skipHistory = 0
				BEGIN
					-- INSERT INTO History Table to maintain Audit Logs
					INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
						(Id, Subject, Operation, IsConstruction, SubjectLastUpdatedOn, LastOperationBy, NextTickOn, NextTickFired, NextTickContext, OurSubscriptions, Version,
							NextSideEffectSeqNumber, SysStart, SysEnd, Tombstone)
						SELECT @id, OldSubject, OldOperation, OldIsConstruction, OldSubjectLastUpdatedOn, OldLastOperationBy, OldNextTickOn, OldNextTickFired, OldNextTickContext, OldOurSubscriptions,
							OldVersion, OldNextSideEffectSeqNum, OldSysStart, SYSDATETIMEOFFSET(), 1
						FROM @outputTable;
				END
				ELSE
				BEGIN
					UPDATE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
					SET Tombstone = 1
					WHERE Id = @id AND Version = (SELECT MAX(Version) FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] WHERE Id = @id)
				END

				SET @offSyncConcurrencyToken = NULL
				SET @isDeleted = 1

				-- Delete Indices
				DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index] WHERE SubjectId = @id;

				___DELETE_SEARCH_INDEX_SQL___

				___DELETE_GEOGRAPHY_INDEX_SQL___

				___DELETE_PROMOTED_INDICES_SQL___

				-- Delete side effect that called this proc, to eliminate risk of orphaned side effect in case of partial failure
				IF @sideEffectId IS NOT NULL
				BEGIN
					DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SideEffect] WHERE SubjectId = @id AND SideEffectId = @sideEffectId;
				END

				-- Delete prepared state, although this proc should not be called inside transaction
				DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared] WHERE Id = @id;

				-- Delete subscriptions, although subscribers will be unaware of it
				DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Subscription] WHERE SubjectId = @id;

				-- Delete blobs owned by the subject
				DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Blob] WHERE SubjectId = @id;

				-- Delete call dedup records
				DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____CallDedup] WHERE SubjectId = @id;
			END

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@trancount > 0 ROLLBACK TRANSACTION;
		;THROW;
	END CATCH