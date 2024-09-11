IF @hasConcurrencyError = 0 AND EXISTS (SELECT TOP 1 1 FROM @newSideEffects)
        BEGIN
            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____SideEffect]
                (SubjectId, SideEffectId, SideEffectTarget, SideEffect, SideEffectSeqNumber, SubjectVersion)
                SELECT @id, Id, SideEffectTarget, SideEffect, SideEffectSeqNumber, @version
                    FROM @newSideEffects;
        END

        ___UPSERT_SEARCH_INDEX_SQL___

        ___UPSERT_GEOGRAPHY_INDEX_SQL___

        IF (@hasConcurrencyError = 0 AND
            EXISTS (SELECT TOP 1 1 FROM @indices WHERE [Kind] IN (1, 2, 3, 4)))
        BEGIN
            DELETE indexTable
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index] AS indexTable
                INNER JOIN @indices input
                    ON
                        input.[Kind]              IN (1, 2, 3, 4) AND
                        input.IsDelete            = 1 AND
                        indexTable.[SubjectId]    = @id AND
                        input.[Key]               = indexTable.[Key] AND
                        (input.[ValueStr]         = indexTable.[ValueStr] OR (input.[ValueStr] IS NULL AND indexTable.[ValueStr] IS NULL)) AND
                        (input.[ValueInt]         = indexTable.[ValueInt] OR (input.[ValueInt] IS NULL AND indexTable.[ValueInt] IS NULL)) AND
                        (
                            (indexTable.[EnforceUniqueness] = 0 AND input.[Kind] IN (1, 3))
                            OR
                            (indexTable.[EnforceUniqueness] = 1 AND input.[Kind] IN (2, 4))
                        );

            -- insert non-unique indices as a batch
            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index]
                ([SubjectId], [Key], [ValueStr], [ValueInt], [EnforceUniqueness])
                    SELECT @id, [Key], [ValueStr], [ValueInt], 0 AS [EnforceUniqueness] FROM
                        @indices WHERE [Kind] IN (1, 3) AND IsDelete = 0;

            IF @subjectTransactionId IS NULL -- usual / non-transactional upsert, insert unique indices
            BEGIN
                -- insert unique indices one by one to get the violated key
                DECLARE @uniqueIndexKey NVARCHAR(80)
                SELECT @uniqueIndexKey = MIN([Key]) FROM @indices
                    WHERE IsDelete = 0 AND [Kind] IN (2, 4)

                WHILE (@violatedUniqueIndexKey IS NULL AND @uniqueIndexKey IS NOT NULL)
                BEGIN
                    BEGIN TRY
                        INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index]
                        ([SubjectId], [Key], [ValueStr], [ValueInt], [EnforceUniqueness])
                        SELECT @id, [Key], [ValueStr], [ValueInt], 1 FROM
                            @indices WHERE IsDelete = 0 AND [Kind] IN (2, 4) AND [Key] = @uniqueIndexKey;
                        -- next
                        SELECT @uniqueIndexKey = MIN([Key])
                            FROM @indices WHERE IsDelete = 0 AND [Kind] IN (2, 4) AND [Key] > @uniqueIndexKey
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
            ELSE  -- transactional upsert, unique indices already inserted but under dummy empty Id, set proper Id
            BEGIN
                IF EXISTS (SELECT * FROM @indices WHERE [Kind] IN (2,4) AND [IsDelete] = 0)
                BEGIN
                    UPDATE indexTable
                    SET indexTable.[SubjectId] = @id
                    FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Index] indexTable
                    INNER JOIN @indices input ON
                        indexTable.[SubjectId] = ''
                        AND indexTable.[EnforceUniqueness] = 1
                        AND input.[Kind] IN (2, 4)
                        AND input.[IsDelete] = 0
                        AND input.[Key] = indexTable.[Key]
                        AND (indexTable.[ValueStr] = input.[ValueStr] OR indexTable.[ValueInt] = input.[ValueInt])
                END
            END

            IF @violatedUniqueIndexKey IS NULL
            BEGIN
                noop1: -- promoted indices, if any go here
___UPSERT_ALL_PROMOTED_INDICES_SQL___
            END
        END

        IF (@hasConcurrencyError = 0 AND @violatedUniqueIndexKey IS NULL AND
           EXISTS (SELECT TOP 1 1 FROM @callDedupActions))
        BEGIN
            -- delete
            DELETE dedupTable
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____CallDedup] AS dedupTable
                INNER JOIN @callDedupActions input
                    ON
                        input.[IsDelete]            = 1 AND
                        input.[IsInsert]            = 0 AND
                        input.[CallerLifeCycleName] = dedupTable.[CallerLifeCycleName] AND
                        input.[CallerId]            = dedupTable.[CallerId] AND
                        dedupTable.[SubjectId]      = @id;
            -- insert
            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____CallDedup]
                ([SubjectId], [CallerLifeCycleName], [CallerId], [CallId], [CalledOn])
                    SELECT @id, [CallerLifeCycleName], [CallerId], [CallId], @subjectLastUpdatedOn FROM
                        @callDedupActions WHERE [IsDelete] = 0 AND [IsInsert] = 1
            -- update
            UPDATE dedupTable
                SET
                    dedupTable.[CallId] = input.[CallId],
                    dedupTable.[CalledOn] = @subjectLastUpdatedOn
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____CallDedup] AS dedupTable
                INNER JOIN @callDedupActions input
                    ON
                        input.[IsDelete]            = 0 AND
                        input.[IsInsert]            = 0 AND
                        input.[CallerLifeCycleName] = dedupTable.[CallerLifeCycleName] AND
                        input.[CallerId]            = dedupTable.[CallerId] AND
                        dedupTable.[SubjectId]      = @id;
        END

        IF (@hasConcurrencyError = 0 AND @violatedUniqueIndexKey IS NULL AND
           EXISTS (SELECT TOP 1 1 FROM @blobActions))
        BEGIN
            DELETE blobTable
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Blob] AS blobTable
                INNER JOIN @blobActions input
                    ON
                        input.[IsDelete]      = 1 AND
                        input.[IsAppend]      = 0 AND
                        input.[Id]            = blobTable.[Id] AND
                        input.[Revision]      = blobTable.[Revision] AND
                        blobTable.[SubjectId] = @id;

            IF @@ROWCOUNT < (SELECT COUNT(*) FROM @blobActions WHERE IsDelete = 1 AND IsAppend = 0)
            BEGIN
                RAISERROR('Blob not found or was changed concurrently, cannot delete.', 11, 1);
            END

            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Blob]
                ([SubjectId], [Id], [Revision], [MimeType], [Data])
                    SELECT @id, [Id], [Revision], [MimeType], [Data] FROM
                        @blobActions WHERE IsDelete = 0 AND IsAppend = 0 AND [Data] IS NOT NULL;

            INSERT INTO [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Blob]
                ([SubjectId], [Id], [Revision], [MimeType], [Data])
                    SELECT @id, ba.[Id], ba.[Revision], ba.[MimeType], tb.[Bytes] FROM
                        @blobActions ba
                        JOIN [___SCHEMA_NAME___].[TransferBlob] tb on tb.[Id] = ba.TransferBlobId
                    WHERE IsDelete = 0 AND IsAppend = 0 AND ba.[TransferBlobId] IS NOT NULL;

            IF @@ROWCOUNT <> (SELECT COUNT(*) FROM @blobActions WHERE IsDelete = 0 AND IsAppend = 0 AND TransferBlobId IS NOT NULL)
            BEGIN
                RAISERROR('Transfer Blob not found', 11, 1);
            END
            ELSE
            BEGIN
                -- DELETE Transfer Blobs
                DELETE FROM [___SCHEMA_NAME___].[TransferBlob] WHERE [Id] IN (
                     SELECT [TransferBlobId] FROM @blobActions
                        WHERE IsDelete = 0 AND IsAppend = 0 AND TransferBlobId IS NOT NULL);
            END

            UPDATE blobTable
                SET
                    blobTable.[Revision] = input.[NewRevision],
                    blobTable.[Data] = blobTable.[Data] + input.[Data]
                FROM [___SCHEMA_NAME___].[___LIFECYCLE_NAME____Blob] AS blobTable
                INNER JOIN @blobActions input
                    ON
                        input.[IsDelete]      = 0 AND
                        input.[IsAppend]      = 1 AND
                        input.[Id]            = blobTable.[Id] AND
                        input.[Revision]      = blobTable.[Revision] AND
                        blobTable.[SubjectId] = @id;

            IF @@ROWCOUNT < (SELECT COUNT(*) FROM @blobActions WHERE IsDelete = 0 AND IsAppend = 1)
            BEGIN
                RAISERROR('Blob not found or was changed concurrently, cannot append.', 11, 1);
            END;
        END