                DELETE promotedIndexTable
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index____PROMOTED_KEY___] AS promotedIndexTable
                INNER JOIN @promotedIndices input
                    ON
                        input.IsDelete                 = 1 AND
                        promotedIndexTable.[SubjectId] = @id AND
                        input.[PromotedKey]            = '___PROMOTED_KEY___' AND
                        input.[PromotedValue]          = promotedIndexTable.[PromotedValue] AND
                        input.[Key]                    = promotedIndexTable.[Key] AND
                        (input.[ValueStr]              = promotedIndexTable.[ValueStr] OR (input.[ValueStr] IS NULL AND promotedIndexTable.[ValueStr] IS NULL)) AND
                        (input.[ValueInt]              = promotedIndexTable.[ValueInt] OR (input.[ValueInt] IS NULL AND promotedIndexTable.[ValueInt] IS NULL))

                -- unlike normal indices, insert both unique and non-unique indices as a batch
                -- (UNIQUE is already enforced by base index table which we already inserted to, promoted table is missing UNIQUE column)
                INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index____PROMOTED_KEY___]
                    ([SubjectId], [PromotedValue], [Key], [ValueStr], [ValueInt])
                        SELECT @id, promotedIndices.[PromotedValue], promotedIndices.[Key], promotedIndices.[ValueStr], promotedIndices.[ValueInt] FROM
                            @promotedIndices promotedIndices
                            WHERE promotedIndices.PromotedKey = '___PROMOTED_KEY___' AND promotedIndices.IsDelete = 0;

