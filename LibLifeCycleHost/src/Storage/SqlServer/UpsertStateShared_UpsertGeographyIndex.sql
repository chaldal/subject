IF (@hasConcurrencyError = 0 AND
            -- Kind 6 - geography
            EXISTS (SELECT TOP 1 1 FROM @indices WHERE [Kind] = 6))
        BEGIN
            DELETE indexTable
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____GeographyIndex] AS indexTable
                INNER JOIN @indices input
                    ON
                        input.[Kind]              = 6 AND
                        input.IsDelete            = 1 AND
                        indexTable.[SubjectId]    = @id AND
                        indexTable.[Key]          = input.[Key];
                        -- No need to check values: GeometryIndex table can't have multiple values for
                        -- the same index key because SubjectId+IndexKey form a PK.

            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____GeographyIndex]
                ([SubjectId], [Key], [ValueGeography])
                SELECT @id, [Key], CONVERT(geography, [ValueStr]) FROM
                    @indices WHERE [Kind] = 6 AND IsDelete = 0;
        END