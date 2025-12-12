insert into "Configuration"."Configurations" ("Code", "Value") values
	('RedisHost', 'checklyst-prd.u2gbnk.ng.0001.euw1.cache.amazonaws.com'),
	('RedisPort', '6379'),
	('SqsBaseUrl', 'https://sqs.eu-west-1.amazonaws.com/'),
	('SqsPrefix', 'prd-'),
	('ApiBaseUrl', 'https://checklystinternal.jnj.com/'),
	('WebApplicationUrl', 'https://checklyst.jnj.com'), --the url must be specified without final slash
	('PublicRepositoryUrl', 'https://checklyst.jnj.com/content/media/'),
	('InstanceCloseTimeout', '4320'); --3 days, expressed in minutes
	('PrototypesStorageBucket', '???'),
	('InstancesStorageBucket', '???'),
	('NotificationsEmailAddress', '???'),
	('ContextualReportTimeout', '60');
  
