IF NOT EXISTS (
    SELECT
        table_name
    FROM
        information_schema.tables
    WHERE
        table_name = 'OrleansQuery' AND table_schema = 'dbo' )
BEGIN
	SET XACT_ABORT ON;
	BEGIN TRANSACTION;

	CREATE TABLE dbo.OrleansQuery
	(
		QueryKey VARCHAR(64) NOT NULL,
		QueryText VARCHAR(8000) NOT NULL,

		CONSTRAINT OrleansQuery_Key PRIMARY KEY(QueryKey)
	);

	-- For each deployment, there will be only one (active) membership version table version column which will be updated periodically.
	CREATE TABLE dbo.OrleansMembershipVersionTable
	(
		DeploymentId NVARCHAR(150) NOT NULL,
		Timestamp DATETIME2(3) NOT NULL DEFAULT GETUTCDATE(),
		Version INT NOT NULL DEFAULT 0,

		CONSTRAINT PK_OrleansMembershipVersionTable_DeploymentId PRIMARY KEY(DeploymentId)
	);

	-- Every silo instance has a row in the membership table.
	CREATE TABLE dbo.OrleansMembershipTable
	(
		DeploymentId NVARCHAR(150) NOT NULL,
		Address VARCHAR(45) NOT NULL,
		Port INT NOT NULL,
		Generation INT NOT NULL,
		SiloName NVARCHAR(150) NOT NULL,
		HostName NVARCHAR(150) NOT NULL,
		Status INT NOT NULL,
		ProxyPort INT NULL,
		SuspectTimes VARCHAR(8000) NULL,
		StartTime DATETIME2(3) NOT NULL,
		IAmAliveTime DATETIME2(3) NOT NULL,

		CONSTRAINT PK_MembershipTable_DeploymentId PRIMARY KEY(DeploymentId, Address, Port, Generation),
		CONSTRAINT FK_MembershipTable_MembershipVersionTable_DeploymentId FOREIGN KEY (DeploymentId) REFERENCES dbo.OrleansMembershipVersionTable (DeploymentId)
	);

	INSERT INTO dbo.OrleansQuery(QueryKey, QueryText)
	VALUES
	(
		'UpdateIAmAlivetimeKey','
		-- This is expected to never fail by Orleans, so return value
		-- is not needed nor is it checked.
		SET NOCOUNT ON;
		UPDATE dbo.OrleansMembershipTable
		SET
			IAmAliveTime = @IAmAliveTime
		WHERE
			DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
			AND Address = @Address AND @Address IS NOT NULL
			AND Port = @Port AND @Port IS NOT NULL
			AND Generation = @Generation AND @Generation IS NOT NULL;
	');

	INSERT INTO dbo.OrleansQuery(QueryKey, QueryText)
	VALUES
	(
		'InsertMembershipVersionKey','
		SET NOCOUNT ON;
		INSERT INTO dbo.OrleansMembershipVersionTable
		(
			DeploymentId
		)
		SELECT @DeploymentId
		WHERE NOT EXISTS
		(
			SELECT 1
			FROM
				dbo.OrleansMembershipVersionTable WITH(HOLDLOCK, XLOCK, ROWLOCK)
			WHERE
				DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
		);
		SELECT @@ROWCOUNT;
	');

	INSERT INTO dbo.OrleansQuery(QueryKey, QueryText)
	VALUES
	(
		'InsertMembershipKey','
		SET XACT_ABORT, NOCOUNT ON;
		DECLARE @ROWCOUNT AS INT;
		BEGIN TRANSACTION;
		INSERT INTO dbo.OrleansMembershipTable
		(
			DeploymentId,
			Address,
			Port,
			Generation,
			SiloName,
			HostName,
			Status,
			ProxyPort,
			StartTime,
			IAmAliveTime
		)
		SELECT
			@DeploymentId,
			@Address,
			@Port,
			@Generation,
			@SiloName,
			@HostName,
			@Status,
			@ProxyPort,
			@StartTime,
			@IAmAliveTime
		WHERE NOT EXISTS
		(
			SELECT 1
			FROM
				dbo.OrleansMembershipTable WITH(HOLDLOCK, XLOCK, ROWLOCK)
			WHERE
				DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
				AND Address = @Address AND @Address IS NOT NULL
				AND Port = @Port AND @Port IS NOT NULL
				AND Generation = @Generation AND @Generation IS NOT NULL
		);
		UPDATE dbo.OrleansMembershipVersionTable
		SET
			Timestamp = GETUTCDATE(),
			Version = Version + 1
		WHERE
			DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
			AND Version = @Version AND @Version IS NOT NULL
			AND @@ROWCOUNT > 0;
		SET @ROWCOUNT = @@ROWCOUNT;
		IF @ROWCOUNT = 0
			ROLLBACK TRANSACTION
		ELSE
			COMMIT TRANSACTION
		SELECT @ROWCOUNT;
	');

	INSERT INTO dbo.OrleansQuery(QueryKey, QueryText)
	VALUES
	(
		'UpdateMembershipKey','
		SET XACT_ABORT, NOCOUNT ON;
		BEGIN TRANSACTION;
		UPDATE dbo.OrleansMembershipVersionTable
		SET
			Timestamp = GETUTCDATE(),
			Version = Version + 1
		WHERE
			DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
			AND Version = @Version AND @Version IS NOT NULL;
		UPDATE dbo.OrleansMembershipTable
		SET
			Status = @Status,
			SuspectTimes = @SuspectTimes,
			IAmAliveTime = @IAmAliveTime
		WHERE
			DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
			AND Address = @Address AND @Address IS NOT NULL
			AND Port = @Port AND @Port IS NOT NULL
			AND Generation = @Generation AND @Generation IS NOT NULL
			AND @@ROWCOUNT > 0;
		SELECT @@ROWCOUNT;
		COMMIT TRANSACTION;
	');

	INSERT INTO dbo.OrleansQuery(QueryKey, QueryText)
	VALUES
	(
		'GatewaysQueryKey','
		SELECT
			Address,
			ProxyPort,
			Generation
		FROM
			dbo.OrleansMembershipTable
		WHERE
			DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL
			AND Status = @Status AND @Status IS NOT NULL
			AND ProxyPort > 0;
	');

	INSERT INTO dbo.OrleansQuery(QueryKey, QueryText)
	VALUES
	(
		'MembershipReadRowKey','
		SELECT
			v.DeploymentId,
			m.Address,
			m.Port,
			m.Generation,
			m.SiloName,
			m.HostName,
			m.Status,
			m.ProxyPort,
			m.SuspectTimes,
			m.StartTime,
			m.IAmAliveTime,
			v.Version
		FROM
			dbo.OrleansMembershipVersionTable v
			-- This ensures the version table will returned even if there is no matching membership row.
			LEFT OUTER JOIN dbo.OrleansMembershipTable m ON v.DeploymentId = m.DeploymentId
			AND Address = @Address AND @Address IS NOT NULL
			AND Port = @Port AND @Port IS NOT NULL
			AND Generation = @Generation AND @Generation IS NOT NULL
		WHERE
			v.DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
	');

	INSERT INTO dbo.OrleansQuery(QueryKey, QueryText)
	VALUES
	(
		'MembershipReadAllKey','
		SELECT
			v.DeploymentId,
			m.Address,
			m.Port,
			m.Generation,
			m.SiloName,
			m.HostName,
			m.Status,
			m.ProxyPort,
			m.SuspectTimes,
			m.StartTime,
			m.IAmAliveTime,
			v.Version
		FROM
			dbo.OrleansMembershipVersionTable v LEFT OUTER JOIN dbo.OrleansMembershipTable m
			ON v.DeploymentId = m.DeploymentId
		WHERE
			v.DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
	');

	INSERT INTO dbo.OrleansQuery(QueryKey, QueryText)
	VALUES
	(
		'DeleteMembershipTableEntriesKey','
		DELETE FROM dbo.OrleansMembershipTable
		WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
		DELETE FROM dbo.OrleansMembershipVersionTable
		WHERE DeploymentId = @DeploymentId AND @DeploymentId IS NOT NULL;
	');

	INSERT INTO OrleansQuery(QueryKey, QueryText)
	VALUES
		(
			'CleanupDefunctSiloEntriesKey','
		DELETE FROM dbo.OrleansMembershipTable
		WHERE DeploymentId = @DeploymentId
			AND @DeploymentId IS NOT NULL
			AND IAmAliveTime < @IAmAliveTime
			AND Status != 3;
	');

	COMMIT;
END;