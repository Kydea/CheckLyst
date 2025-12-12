insert into "Configuration"."Configurations" ("Code", "Value") values
	('RedisHost', 'redis-02.clm.internal'),
	('RedisPort', '6379'),
	('SqsBaseUrl', ''),
	('SqsPrefix', ''),
	('ApiBaseUrl', 'http://api-02.clm.internal/'),
	('WebApplicationUrl', 'http://clm-02.matematici-aws.com'), --the url must be specified without final slash
	('PublicRepositoryUrl', 'https://s3-eu-west-1.amazonaws.com/public-02.matematici-clm.com/'), -- TODO ??
	('InstanceCloseTimeout', '4320'), --3 days, expressed in minutes
	('PrototypesStorageBucket', '???'),
	('InstancesStorageBucket', '???'),
	('NotificationsEmailAddress', '??@matematici.com'),
	('ContextualReportTimeout', '60'); --seconds
