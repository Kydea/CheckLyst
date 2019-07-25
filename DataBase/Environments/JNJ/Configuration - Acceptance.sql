insert into "Configuration"."Configurations" ("Code", "Value") values
	('RedisHost', 'checklyst-qa.u2gbnk.ng.0001.euw1.cache.amazonaws.com'),
	('RedisPort', '6379'),
	('SqsBaseUrl', 'https://sqs.eu-west-1.amazonaws.com/'),
	('SqsPrefix', 'qa-'),
	('ApiBaseUrl', 'https://internal-checklystqa.jnj.com/'),
	('WebApplicationUrl', 'https://checklystqa.jnj.com'), --the url must be specified without final slash
	('PublicRepositoryUrl', 'https://checklystqa.jnj.com/content/media/'),
	('InstanceCloseTimeout', '4320'), --3 days, expressed in minutes
	('PrototypesStorageBucket', '???'),
	('InstancesStorageBucket', '???');