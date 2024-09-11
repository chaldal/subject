CREATE OR ALTER PROCEDURE [___SCHEMA_NAME___].[_SequenceNextValue] (@sequenceName  NVARCHAR(100))
AS
BEGIN
    DECLARE @nextValueSql NVARCHAR(MAX)
    SET @nextValueSql = N'SELECT NEXT VALUE FOR [___SCHEMA_NAME___].[_Sequence_' + @sequenceName + N']'

    BEGIN TRY
       EXEC sp_executesql @nextValueSql
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 208 -- Invalid object name i.e. sequence doesn't exist
        BEGIN
            BEGIN TRY
                -- create starting with -- (-2)^63 + 1, i.e. min value of BIGINT + 1
                DECLARE @sql NVARCHAR(MAX)
                SET @sql = N'CREATE SEQUENCE [___SCHEMA_NAME___].[_Sequence_' + @sequenceName + N'] AS BIGINT START WITH -9223372036854775807'
                EXEC sp_executesql @sql
            END TRY
            BEGIN CATCH
                IF ERROR_NUMBER () <> 2714 THROW; -- There is already an object named i.e. sequence created concurrently
            END CATCH
            EXEC sp_executesql @nextValueSql
        END
        ELSE THROW;
    END CATCH
END
