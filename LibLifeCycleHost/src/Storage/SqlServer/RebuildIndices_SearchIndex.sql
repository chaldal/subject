-- do NOT convert to single DELETE statement with "OR", it will be very slow on large tables
DELETE indexTable
    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SearchIndex] AS indexTable
    INNER JOIN @indices input
        ON
            (@removeAllNonMatchingKeys = 1 AND
                -- starts-with LIKE should make it faster,
                -- Match by indexKey.[SubjectId] alone will lead to full scan (should we index the column?)
                indexTable.[Id] LIKE (input.[Id] + '%') AND indexTable.[SubjectId] = input.[Id]);
DELETE indexTable
    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SearchIndex] AS indexTable
    INNER JOIN @indices input
        ON
            (@removeAllNonMatchingKeys = 0 AND indexTable.[Id] = input.[Id] + '_' + input.[Key]);

INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SearchIndex]
([Id], [SubjectId], [Key], [ValueStr])
SELECT [Id] + '_' + [Key], [Id], [Key], [ValueStr] FROM
    @indices WHERE IsDelete = 0 AND [Kind] = 5 AND PromotedKey = '';

