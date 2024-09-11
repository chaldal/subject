DELETE indexTable
    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____GeographyIndex] AS indexTable
    INNER JOIN @indices input
        ON
            (@removeAllNonMatchingKeys = 1 AND indexTable.[SubjectId] = input.[Id])
            OR
            (@removeAllNonMatchingKeys = 0 AND indexTable.[SubjectId] = input.[Id] AND indexTable.[Key] = input.[Key]);

INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____GeographyIndex]
([SubjectId], [Key], [ValueGeography])
SELECT [Id], [Key], CONVERT(geography, [ValueStr]) FROM
    @indices WHERE IsDelete = 0 AND [Kind] = 6 AND PromotedKey = '';

