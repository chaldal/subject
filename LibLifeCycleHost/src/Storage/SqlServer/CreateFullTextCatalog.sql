IF NOT EXISTS (
	SELECT
		1
	FROM
		sys.fulltext_catalogs
	WHERE
		[name] = 'SubjectSearchCatalog')
BEGIN
	CREATE FULLTEXT CATALOG SubjectSearchCatalog;
END