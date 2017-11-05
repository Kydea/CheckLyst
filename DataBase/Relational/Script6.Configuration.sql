create schema "Configuration";

create table "Configuration"."Configurations"
(
	"Code" varchar (100),
	
	constraint "Configurations_PK" primary key ("Code"),
	
	"Value" varchar (500) not null,

	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedOn" timestamptz
);

insert into "Configuration"."Configurations" ("Code", "Value") values
	('RedisHost', 'redis-01.clm.internal'),
	('RedisPort', '6379'),
	('SqsBaseUrl', 'https://sqs.eu-west-1.amazonaws.com/'),
	('ApiBaseUrl', 'http://api.clm.internal/');