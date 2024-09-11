IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = 'TimeSeries____TIMESERIES_NAME___' AND table_schema = '___SCHEMA_NAME___')
BEGIN
    DECLARE @stateTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[TimeSeries____TIMESERIES_NAME___]
        (
            -- length of of Id+IndexKey+IndexVal is not arbitrary, it must satisfy 900 bytes limit of clustered index
            [Id]           NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [IndexKey]     NVARCHAR(60)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [IndexVal]     NVARCHAR(300)    COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [TimeIndex]    DATETIMEOFFSET   NOT NULL,
            [Hash]         INT              NOT NULL,
            [Value]        FLOAT            NOT NULL,
            [DataPoint]    VARBINARY(MAX)   NOT NULL,
            [CreatedBy]    NVARCHAR(80)     COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [SysCreatedOn] DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
            CONSTRAINT PK_TimeSeries____TIMESERIES_NAME___ PRIMARY KEY ([Id], [IndexKey], [IndexVal], [TimeIndex], [Hash])
        );';

    -- index by time only, to allow partition away old data
    -- index by Id-then-timeIndex-then-IndexKey, to allow group by Index value
    DECLARE @indicesSql NVARCHAR(MAX) = '
        CREATE NONCLUSTERED INDEX [IX_TimeSeries____TIMESERIES_NAME____Time] ON [___SCHEMA_NAME___].[TimeSeries____TIMESERIES_NAME___] ([TimeIndex]);
        CREATE NONCLUSTERED INDEX [IX_TimeSeries____TIMESERIES_NAME____Id_TimeIndex_IndexKey] ON [___SCHEMA_NAME___].[TimeSeries____TIMESERIES_NAME___] ([Id], [TimeIndex], [IndexKey]);';

    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    EXEC sp_executesql @stateTableSql WITH RESULT SETS NONE;
    EXEC sp_executesql @indicesSql    WITH RESULT SETS NONE;
    COMMIT;
END;

