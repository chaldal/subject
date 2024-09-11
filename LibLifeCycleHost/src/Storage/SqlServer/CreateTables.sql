IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = '___LIFECYCLE_NAME___' AND table_schema = '___SCHEMA_NAME___')
BEGIN
    DECLARE @versionSeqSql NVARCHAR(MAX) = '
        CREATE SEQUENCE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Version]
        AS BIGINT
        START WITH 0;';

    DECLARE @stateTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
        (
            [Id]                      NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL CONSTRAINT PK____LIFECYCLE_NAME___ PRIMARY KEY,
            [Subject]                 VARBINARY(MAX)   NOT NULL,
            [Operation]               VARBINARY(MAX)   NOT NULL,
            [IsConstruction]          BIT              NOT NULL,
            [SubjectLastUpdatedOn]    DATETIMEOFFSET   NOT NULL,
            [LastOperationBy]         NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [NextTickOn]              DATETIMEOFFSET,
            [NextTickFired]           BIT,
            [NextTickContext]         VARBINARY(MAX),
            [OurSubscriptions]        VARBINARY(MAX)   NOT NULL,
            [ConcurrencyToken]        ROWVERSION       NOT NULL,
            [Version]                 BIGINT           NOT NULL,
            [SysStart]                DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
            [NextSideEffectSeqNumber] BIGINT           NOT NULL,
            -- Required for Custom Reminder Table
            [GrainIdHash]             INT              NOT NULL
        );';

    DECLARE @preparedStateTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Prepared]
        (
            [Id]                                 NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL CONSTRAINT PK____LIFECYCLE_NAME____Prepared PRIMARY KEY,
            [PreparedTransactionalState]         VARBINARY(MAX)   NOT NULL,
            [SubjectTransactionId]               UNIQUEIDENTIFIER NOT NULL,
            [SysStart]                           DATETIMEOFFSET   NOT NULL CONSTRAINT DF____LIFECYCLE_NAME____Prepared_SysStart DEFAULT SYSDATETIMEOFFSET(),
            [PreparedInitializeConcurrencyToken] ROWVERSION       NOT NULL
        );';

    DECLARE @historyTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
        (
            [Id]                      NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [Subject]                 VARBINARY(MAX)   NOT NULL,
            [Operation]               VARBINARY(MAX)   NOT NULL,
            [IsConstruction]          BIT              NOT NULL,
            [SubjectLastUpdatedOn]    DATETIMEOFFSET   NOT NULL,
            [LastOperationBy]         NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [NextTickOn]              DATETIMEOFFSET,
            [NextTickFired]           BIT,
            [NextTickContext]         VARBINARY(MAX),
            [OurSubscriptions]        VARBINARY(MAX)   NOT NULL,
            [Version]                 BIGINT           NOT NULL,
            [NextSideEffectSeqNumber] BIGINT           NOT NULL,
            [SysStart]                DATETIMEOFFSET   NOT NULL,
            [SysEnd]                  DATETIMEOFFSET   NOT NULL,
            [Tombstone]               BIT              NULL
            CONSTRAINT PK____LIFECYCLE_NAME____History PRIMARY KEY([Id], [Version])
        );';

    DECLARE @indexTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index]
        (
            [SubjectId]         NVARCHAR(80)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [Key]               NVARCHAR(80)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [ValueStr]          NVARCHAR(500) COLLATE Latin1_General_CS_AS_KS_WS NULL,
            [ValueInt]          BIGINT NULL,
            [EnforceUniqueness] BIT NOT NULL DEFAULT 0
        );';

    DECLARE @subscriptionsTable NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Subscription]
        (
            [SubjectId]         NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [SubscriptionName]  NVARCHAR(200)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [LifeCycleName]     NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [SubscriberId]      NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [LifeEvent]         VARBINARY(MAX) NOT NULL,
            CONSTRAINT PK____LIFECYCLE_NAME____Subscription PRIMARY KEY ([SubjectId], [SubscriptionName], [LifeCycleName], [SubscriberId])
        );';

    DECLARE @indicesSql NVARCHAR(MAX) = '
        CREATE CLUSTERED INDEX [CX____LIFECYCLE_NAME____Index] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index] ([SubjectId],[Key]);
        CREATE NONCLUSTERED INDEX [IX____LIFECYCLE_NAME____Index_KeyValues] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index] ([Key], [ValueStr], [ValueInt] DESC);
        CREATE UNIQUE INDEX [IX____LIFECYCLE_NAME____Index_UniqueValues] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index] ([Key], [ValueStr], [ValueInt]) WHERE [EnforceUniqueness] = 1;

        CREATE NONCLUSTERED INDEX [IX____LIFECYCLE_NAME____GrainIdHashNextTickOn] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] ([GrainIdHash] ASC, [NextTickOn] ASC, [NextTickFired] ASC) WHERE [NextTickOn] IS NOT NULL;
        CREATE NONCLUSTERED INDEX [IX____LIFECYCLE_NAME____Version] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME___] ([Version]);
        CREATE NONCLUSTERED INDEX [IX____LIFECYCLE_NAME____History_Version] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] ([Version]) INCLUDE ([SysStart]);
        CREATE NONCLUSTERED INDEX [IX____LIFECYCLE_NAME____History_TombstoneSysEnd] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] ([SysEnd]) WHERE [Tombstone] = 1;';

    DECLARE @sideEffectTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SideEffect]
        (
            [SubjectId]               NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [SideEffectId]            UNIQUEIDENTIFIER NOT NULL,
            [SideEffectSeqNumber]     BIGINT           NOT NULL,
            [SideEffect]              VARBINARY(MAX)   NOT NULL,
            [SideEffectTarget]        NVARCHAR(MAX)    COLLATE Latin1_General_CS_AS_KS_WS NULL,
            [SubjectVersion]          BIGINT           NOT NULL,
            [CreatedOn]               DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
            [FailureReason]           NVARCHAR(MAX)    NULL,
            [FailureSeverity]         TINYINT          NULL,
            [FailureAckedUntil]       DATETIMEOFFSET   NULL,
            CONSTRAINT PK____LIFECYCLE_NAME____SideEffect PRIMARY KEY ([SubjectId], [SideEffectId])
        );';

    DECLARE @blobTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Blob]
        (
            [SubjectId] NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [Id]        UNIQUEIDENTIFIER NOT NULL,
            [Revision]  INT              NOT NULL,
            [MimeType]  VARCHAR(127)     NULL,
            [Data]      VARBINARY(MAX)   NOT NULL,
            CONSTRAINT PK____LIFECYCLE_NAME____Blob PRIMARY KEY ([SubjectId], [Id])
        );';

    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    EXEC sp_executesql @versionSeqSql         WITH RESULT SETS NONE;
    EXEC sp_executesql @stateTableSql         WITH RESULT SETS NONE;
    EXEC sp_executesql @preparedStateTableSql WITH RESULT SETS NONE;
    EXEC sp_executesql @historyTableSql       WITH RESULT SETS NONE;
    EXEC sp_executesql @indexTableSql         WITH RESULT SETS NONE;
    EXEC sp_executesql @indicesSql            WITH RESULT SETS NONE;
    EXEC sp_executesql @sideEffectTableSql    WITH RESULT SETS NONE;
    EXEC sp_executesql @subscriptionsTable    WITH RESULT SETS NONE;
    EXEC sp_executesql @blobTableSql          WITH RESULT SETS NONE;
    COMMIT;

END;

-- separate transaction to upgrade existing databases
IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = '___LIFECYCLE_NAME____CallDedup' AND table_schema = '___SCHEMA_NAME___')
BEGIN
    DECLARE @callDedupTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____CallDedup]
        (
            [SubjectId]               NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [CallerLifeCycleName]     NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [CallerId]                NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [CallId]                  UNIQUEIDENTIFIER NOT NULL,
            [CalledOn]                DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
            CONSTRAINT PK____LIFECYCLE_NAME____CallDedup PRIMARY KEY ([SubjectId], [CallerLifeCycleName], [CallerId])
        );';

    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    EXEC sp_executesql @callDedupTableSql WITH RESULT SETS NONE;
    COMMIT;
END;

BEGIN
    DECLARE @historyWithCurrentViewSql NVARCHAR(MAX) = '
        CREATE OR ALTER VIEW [___SCHEMA_NAME___].[___LIFECYCLE_NAME____HistoryWithCurrent]
        AS
            SELECT
                [Id],
                [Subject],
                [Operation],
                [IsConstruction],
                [SubjectLastUpdatedOn],
                [LastOperationBy],
                [NextTickOn],
                [NextTickFired],
                [NextTickContext],
                [OurSubscriptions],
                [Version],
                [NextSideEffectSeqNumber],
                [SysStart],
                [SysEnd],
                [Tombstone]
            FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History]
            UNION ALL
            SELECT
                [Id],
                [Subject],
                [Operation],
                [IsConstruction],
                [SubjectLastUpdatedOn],
                [LastOperationBy],
                [NextTickOn],
                [NextTickFired],
                [NextTickContext],
                [OurSubscriptions],
                [Version],
                [NextSideEffectSeqNumber],
                [SysStart],
                NULL, -- no SysEnd in main subject table
                NULL -- no Tombstone in main subject table
            FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME___]
        ;';

    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    EXEC sp_executesql @historyWithCurrentViewSql WITH RESULT SETS NONE;
    COMMIT;
END;
