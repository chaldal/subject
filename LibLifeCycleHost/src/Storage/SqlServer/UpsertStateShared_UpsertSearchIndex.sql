IF (@hasConcurrencyError = 0 AND
            -- Kind 5 - string search
            EXISTS (SELECT TOP 1 1 FROM @indices WHERE [Kind] = 5))
        BEGIN
            DELETE indexTable
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SearchIndex] AS indexTable
                INNER JOIN @indices input
                    ON
                        input.[Kind]              = 5 AND
                        input.IsDelete            = 1 AND
                        -- join by [Id] not by [Key] and [SubjectId] to avoid range locks and table scans
                        indexTable.[Id]           = @id + '_' + input.[Key];
                        -- No need to check values: SearchIndex table can't have multiple values for
                        -- the same index key because SubjectId+IndexKey form a PK.

            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SearchIndex]
                ([Id], [SubjectId], [Key], [ValueStr])
                SELECT @id + '_' + [Key], @id, [Key], [ValueStr] FROM
                    @indices WHERE [Kind] = 5 AND IsDelete = 0;
        END