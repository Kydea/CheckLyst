insert into "Configuration"."Configurations" ("Code", "Value") values
	('RedisHost', 'checklyst-dev.u2gbnk.ng.0001.euw1.cache.amazonaws.com'),
	('RedisPort', '6379'),
	('SqsBaseUrl', 'https://sqs.eu-west-1.amazonaws.com/'),
	('SqsPrefix', 'dev-'),
	('ApiBaseUrl', 'http://10.157.83.252/'),
	('WebApplicationUrl', '?'), --the url must be specified without final slash
	('PublicRepositoryUrl', 'http://10.157.83.244/content/media/'),
	('InstanceCloseTimeout', '4320'), --3 days, expressed in minutes
	('PrototypesStorageBucket', '???'),
	('InstancesStorageBucket', '???');
