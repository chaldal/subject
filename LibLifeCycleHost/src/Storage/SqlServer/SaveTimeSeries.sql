CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[TimeSeries____TIMESERIES_NAME____Save] (
    @createdBy NVARCHAR(80),
    @points    [___SCHEMA_NAME___].TimeSeriesPointsList READONLY
)
AS
    SET XACT_ABORT, NOCOUNT ON;

    INSERT INTO [___SCHEMA_NAME___].[TimeSeries____TIMESERIES_NAME___]
        ([Id], [IndexKey], [IndexVal], [TimeIndex], [Hash], [Value], [DataPoint], [CreatedBy])
    -- just no-op rows that are already inserted (e.g. a retry after transient exception)
    SELECT [Id], [IndexKey], [IndexVal], [TimeIndex], [Hash], [Value], [DataPoint], @createdBy
    FROM @points p
    WHERE NOT EXISTS
        (SELECT * FROM [___SCHEMA_NAME___].[TimeSeries____TIMESERIES_NAME___]
         WHERE
            [Id] = p.[Id]
            AND [IndexKey] = p.[IndexKey]
            AND [IndexVal] = p.[IndexVal]
            AND [TimeIndex] = p.[TimeIndex]
            AND [Hash] = p.[Hash])