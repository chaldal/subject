IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = '___LIFECYCLE_NAME____GeographyIndex' AND table_schema = '___SCHEMA_NAME___')
BEGIN

    /*
    There was an idea to future-proof this table and create both ValueGeography and ValueGeometry nullable columns,
    so we can add 'SubjectGeometryIndex later, however Geometry requires explicit BOUNDING_BOX i.e. specific to a
    a use case, we can't do it generically.
     */

    DECLARE @geographyIndexTableSql NVARCHAR(MAX) = '
        CREATE TABLE [___SCHEMA_NAME___].[___LIFECYCLE_NAME____GeographyIndex]
        (
            [SubjectId]      NVARCHAR(80)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [Key]            NVARCHAR(80)  COLLATE Latin1_General_CS_AS_KS_WS NOT NULL,
            [ValueGeography] GEOGRAPHY     NOT NULL,
            CONSTRAINT PK____LIFECYCLE_NAME____GeographyIndex PRIMARY KEY ([SubjectId], [Key])
        );';

    DECLARE @indicesSql NVARCHAR(MAX) = '
        CREATE SPATIAL INDEX [IX____LIFECYCLE_NAME____GeographyIndex] ON [___SCHEMA_NAME___].[___LIFECYCLE_NAME____GeographyIndex] ([ValueGeography]);';


    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    EXEC sp_executesql @geographyIndexTableSql      WITH RESULT SETS NONE;
    EXEC sp_executesql @indicesSql            WITH RESULT SETS NONE;
    COMMIT;
END;
