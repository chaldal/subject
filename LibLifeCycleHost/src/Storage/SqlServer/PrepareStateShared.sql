IF @hasConcurrencyError = 0
        BEGIN
            -- reserve unique indices one by one to get the violated key
            DECLARE @uniqueIndexKey NVARCHAR(80)
            SELECT @uniqueIndexKey = MIN([Key]) FROM @uniqueIndicesToReserve
                WHERE IsDelete = 0 AND [Kind] IN (2, 4)

            WHILE (@violatedUniqueIndexKey IS NULL AND @uniqueIndexKey IS NOT NULL)
            BEGIN
                BEGIN TRY
                    INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index]
                    ([SubjectId], [Key], [ValueStr], [ValueInt], [EnforceUniqueness])
                    SELECT '', [Key], [ValueStr], [ValueInt], 1 FROM
                        @uniqueIndicesToReserve WHERE [Kind] IN (2, 4) AND IsDelete = 0 AND [Key] = @uniqueIndexKey;
                    -- next
                    SELECT @uniqueIndexKey = MIN([Key])
                        FROM @uniqueIndicesToReserve WHERE [Kind] IN (2, 4) AND IsDelete = 0 AND [Key] > @uniqueIndexKey
                END TRY
                BEGIN CATCH
                    DECLARE @errNumUniqueIndex INT
                    SET @errNumUniqueIndex = ERROR_NUMBER()
                    IF @errNumUniqueIndex IN (2601, 2627) -- unique index or constraint violation
                    BEGIN
                        SET @violatedUniqueIndexKey = @uniqueIndexKey
                        IF @@trancount > 0 ROLLBACK TRANSACTION;
                    END
                    ELSE THROW;
                END CATCH
            END
        END