CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____ReEncodeHistory] (
    @data [___SCHEMA_NAME___].[ReEncodeSubjectsList] READONLY
)
AS
    UPDATE h
    SET
        [Subject] = ISNULL(input.Subject, h.Subject),
        [Operation] = ISNULL(input.Operation, h.Operation),
        [OurSubscriptions] = ISNULL(input.OurSubscriptions, h.OurSubscriptions)
    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____History] h
    JOIN @data input ON input.Id = h.Id AND input.Version = h.Version

