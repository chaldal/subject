    DELETE promotedIndexTable
        FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index____PROMOTED_KEY___] AS promotedIndexTable
        INNER JOIN @indices input
            ON
                input.[Id] = promotedIndexTable.[SubjectId]
                AND (@removeAllNonMatchingKeys = 1 OR input.[Key] = promotedIndexTable.[Key]);
    INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index____PROMOTED_KEY___]
        ([SubjectId], [PromotedValue], [Key], [ValueStr], [ValueInt])
            SELECT [Id], [PromotedValue], [Key], [ValueStr], [ValueInt] FROM
                @indices WHERE [PromotedKey] = '___PROMOTED_KEY___' AND IsDelete = 0;

