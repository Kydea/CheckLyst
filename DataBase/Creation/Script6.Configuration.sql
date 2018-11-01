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
	('SqsPrefix', ''),
	('ApiBaseUrl', 'http://api.clm.internal/'),
	('PublicRepositoryUrl', 'https://s3-eu-west-1.amazonaws.com/public.matematici-clm.com/'),
	('InstanceCloseTimeout', '4320'); --3 days, expressed in minutes


create table "Configuration"."Settings"
(
	"Code" varchar (100),
	constraint "Settings_PK" primary key ("Code"),

	"Description" text,
	"DefaultValue" text,

	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedOn" timestamptz
);

insert into "Configuration"."Settings" ("Code") values
	('InstancesFtpConnection'),
	('PrototypesFtpConnection'),
	('LDAPHost'),
	('SmtpHost'),
	('SmtpPort'),
	('SmtpUsername'),
	('SmtpPassword'),
	('EmailSender'),
	('UserBlockMessage'),
	('UserBlockWarningMessage'),
	('SignatureMessage'),
	('DecimalSeparator');

create table "Configuration"."SettingValues"
(
	"ID" bigserial primary key,

	"Company" bigint not null references "Infrastructure"."Companies",
	"Setting" varchar (100) not null references "Configuration"."Settings",

	"Value" text not null,

	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedOn" timestamptz
);

create unique index "Index_SettingValues_Company_Setting" on "Configuration"."SettingValues" ("Company", "Setting");