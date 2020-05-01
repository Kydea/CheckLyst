insert into "Configuration"."Configurations" ("Code", "Value") values
	('RedisHost', 'redis-02.clm.internal'),
	('RedisPort', '6379'),
	('SqsBaseUrl', ''),
	('SqsPrefix', ''),
	('ApiBaseUrl', 'http://api-abbvie.clm.internal/'),
	('WebApplicationUrl', 'https://abbvie.clm.matematici-aws.com'), --the url must be specified without final slash
	('PublicRepositoryUrl', 'https://abbvie.clm.matematici-aws.com/Public/'),
	('InstanceCloseTimeout', '4320'), --3 days, expressed in minutes
	('PrototypesStorageBucket', '\\10.0.16.10\Data\Prototypes\'),
	('InstancesStorageBucket', '\\10.0.16.10\Data\Instances\'),
	('NotificationsEmailAddress', '??@matematici.com');
