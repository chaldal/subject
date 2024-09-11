IF TYPE_ID('___SCHEMA_NAME___.SideEffectList_V2') IS NULL
BEGIN
    DECLARE @sideEffectsSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[SideEffectList_V2] AS TABLE (
            [Id]                  UNIQUEIDENTIFIER NOT NULL,
            [SideEffectTarget]    NVARCHAR(MAX) COLLATE Latin1_General_CS_AS_KS_WS NULL,
            [SideEffect]          VARBINARY(MAX)   NOT NULL,
            [SideEffectSeqNumber] BIGINT           NOT NULL
        )';

    EXEC sp_executesql @sideEffectsSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.SubscriptionNameAndLifeEventList') IS NULL
BEGIN
    DECLARE @subscriptionNameAndLifeEventSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[SubscriptionNameAndLifeEventList] AS TABLE (
            [SubscriptionName]  NVARCHAR(200)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [LifeEvent]         VARBINARY(MAX) NOT NULL
        )';

    EXEC sp_executesql @subscriptionNameAndLifeEventSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.SubscriptionNameList') IS NULL
BEGIN
    DECLARE @subscriptionNameSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[SubscriptionNameList] AS TABLE (
            [SubscriptionName] NVARCHAR(200) COLLATE Latin1_General_CS_AS_KS_WS NOT NULL
        )';

    EXEC sp_executesql @subscriptionNameSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.IndexingList_V3') IS NULL
BEGIN
/*
    [Kind] column valid values:
    type private IndexKindCode =
    | NumericNonUnique = 1
    | NumericUnique = 2
    | StringNonUnique = 3
    | StringUnique = 4
    | StringSearch = 5
    | Geography = 6
*/
    DECLARE @indexingListSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[IndexingList_V3] AS TABLE (
            [Key]      NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [ValueStr] NVARCHAR(MAX)  COLLATE Latin1_General_CS_AS_KS_WS NULL,
            [ValueInt] BIGINT         NULL,
            [Kind]     INT            NOT NULL,
            [IsDelete] BIT            NOT NULL
        )';

    EXEC sp_executesql @indexingListSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.RebuildIndexList_V4') IS NULL
BEGIN
    -- RebuildIndexList_V4 Kind codes same as IndexingList_V3
    -- promoted tables used if PromotedKey is non empty
    DECLARE @rebuildIndexList NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[RebuildIndexList_V4] AS TABLE (
            [PromotedKey]       NVARCHAR(240)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [PromotedValue]     NVARCHAR(100)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [Id]                NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [ConcurrencyToken]  BINARY(8)      NOT NULL,
            [Key]               NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [ValueStr]          NVARCHAR(MAX)  COLLATE Latin1_General_CS_AS_KS_WS NULL,
            [ValueInt]          BIGINT         NULL,
            [Kind]              INT            NOT NULL,
            [IsDelete]          BIT            NOT NULL
        )';

    EXEC sp_executesql @rebuildIndexList WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.PromotedIndexingList') IS NULL
BEGIN
    DECLARE @promotedIndexingList NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[PromotedIndexingList] AS TABLE (
            [PromotedKey]       NVARCHAR(240)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [PromotedValue]     NVARCHAR(100)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [Key]               NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [ValueStr]          NVARCHAR(500)  COLLATE Latin1_General_CS_AS_KS_WS NULL,
            [ValueInt]          BIGINT         NULL,
            [IsDelete]          BIT            NOT NULL
        )';

    EXEC sp_executesql @promotedIndexingList WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.IdList') IS NULL
BEGIN
    DECLARE @idListSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[IdList] AS TABLE (
            [Id] NVARCHAR(80) COLLATE Latin1_General_CS_AS_KS_WS NOT NULL PRIMARY KEY
        )';

    EXEC sp_executesql @idListSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.GuidList') IS NULL
BEGIN
    DECLARE @guidListSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[GuidList] AS TABLE (
            [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
        )';

    EXEC sp_executesql @guidListSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.BlobActionList_V2') IS NULL
BEGIN
    DECLARE @blobActionListSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[BlobActionList_V2] AS TABLE (
            [Id]          UNIQUEIDENTIFIER NOT NULL,
            [Revision]    INT              NOT NULL,
            [NewRevision] INT              NULL,
            [MimeType]    VARCHAR(127)     NULL,
            [Data]        VARBINARY(MAX)   NULL,
            [IsDelete]    BIT              NOT NULL,
            [IsAppend]    BIT              NOT NULL,
            [TransferBlobId] UNIQUEIDENTIFIER NULL
        )';

EXEC sp_executesql @blobActionListSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.CallDedupList_V2') IS NULL
BEGIN
    DECLARE @callDedupListSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[CallDedupList_V2] AS TABLE (
            [CallerLifeCycleName]     NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [CallerId]                NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [CallId]                  UNIQUEIDENTIFIER NULL,
            [IsInsert]                BIT              NOT NULL,
            [IsDelete]                BIT              NOT NULL
        )';

EXEC sp_executesql @callDedupListSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.ReEncodeSubjectsList') IS NULL
BEGIN
    DECLARE @reEncodeSubjectsListSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[ReEncodeSubjectsList] AS TABLE (
            [Id]               NVARCHAR(80) COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [Version]          BIGINT         NOT NULL,
            [Subject]          VARBINARY(MAX) NULL,
            [Operation]        VARBINARY(MAX) NULL,
            [OurSubscriptions] VARBINARY(MAX) NULL,
            [ConcurrencyToken] BINARY(8)      NULL
        )';

EXEC sp_executesql @reEncodeSubjectsListSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.ReEncodeSubscriptionsList') IS NULL
BEGIN
    DECLARE @reEncodeSubscriptionsListSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[ReEncodeSubscriptionsList] AS TABLE (
            [SubjectId]        NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [SubscriptionName] NVARCHAR(200)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [LifeCycleName]    NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [SubscriberId]     NVARCHAR(80)   COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [LifeEvent]        VARBINARY(MAX) NOT NULL
        )';

EXEC sp_executesql @reEncodeSubscriptionsListSql WITH RESULT SETS NONE;
END

IF TYPE_ID('___SCHEMA_NAME___.TimeSeriesPointsList') IS NULL
BEGIN
    DECLARE @timeSeriesPointsListSql NVARCHAR(MAX) = '
        CREATE TYPE [___SCHEMA_NAME___].[TimeSeriesPointsList] AS TABLE (
            [Id]        NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [IndexKey]  NVARCHAR(60)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [IndexVal]  NVARCHAR(300)    COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [TimeIndex] DATETIMEOFFSET   NOT NULL,
            [Hash]      INT              NOT NULL,
            [Value]     FLOAT            NOT NULL,
            [DataPoint] VARBINARY(MAX)   NOT NULL
        )';

EXEC sp_executesql @timeSeriesPointsListSql WITH RESULT SETS NONE;
END
