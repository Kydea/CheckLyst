insert into "Configuration"."Configurations" ("Code", "Value") values
	('RedisHost', 'redis-01.clm.internal'),
	('RedisPort', '6379'),
	('SqsBaseUrl', 'https://sqs.eu-west-1.amazonaws.com/'),
	('SqsPrefix', ''),
	('ApiBaseUrl', 'http://api.clm.internal/'),
	('WebApplicationUrl', 'http://clm.matematici-aws.com'), --the url must be specified without final slash
	('PublicRepositoryUrl', 'https://s3-eu-west-1.amazonaws.com/public.matematici-clm.com/'),
	('InstanceCloseTimeout', '4320'), --3 days, expressed in minutes
	('PrototypesStorageBucket', '???'),
	('InstancesStorageBucket', '???');
