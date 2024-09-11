				-- search by [Id] to avoid table scans
				DELETE FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SearchIndex] WHERE ([Id] like @id + '_%') AND SubjectId = @id;