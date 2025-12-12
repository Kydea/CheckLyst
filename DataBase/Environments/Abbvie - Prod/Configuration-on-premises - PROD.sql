insert into "Configuration"."Configurations" ("Code", "Value") values
	('RedisHost', '10.47.183.2'),
	('RedisPort', '6379'),
	('SqsBaseUrl', ''),
	('SqsPrefix', ''),
	('ApiBaseUrl', 'http://checklistcv.abbvienet.com/Api/'),
	('WebApplicationUrl', 'http://checklistcv.abbvienet.com'), --the url must be specified without final slash
	('PublicRepositoryUrl', 'http://checklistcv.abbvienet.com/Public/'),
	('InstanceCloseTimeout', '4320'), --3 days, expressed in minutes
	('PrototypesStorageBucket', '\\10.47.183.36\Data\Prototypes\'),
	('InstancesStorageBucket', '\\10.47.183.36\Data\Instances\'),
	('NotificationsEmailAddress', 'checklistcv@abbvie.com'),
	('ContextualReportTimeout', '60');

