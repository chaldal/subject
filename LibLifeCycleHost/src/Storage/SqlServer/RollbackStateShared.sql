IF @hasConcurrencyError = 0
        BEGIN
            IF EXISTS (SELECT * FROM @uniqueIndicesToRelease WHERE [Kind] IN (2, 4) AND [IsDelete]=1)
            BEGIN
                DELETE FROM indexTable
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index] indexTable
                INNER JOIN @uniqueIndicesToRelease input ON
                    indexTable.[SubjectId] = ''
                    AND indexTable.[EnforceUniqueness] = 1
                    AND input.[IsDelete] = 1
                    AND input.[Kind] IN (2, 4)
                    AND input.[Key] = indexTable.[Key]
                    AND (indexTable.[ValueStr] = input.[ValueStr] OR indexTable.[ValueInt] = input.[ValueInt])
            END
        END