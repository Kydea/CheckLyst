ocreate schema "Configuration";

create table "Configuration"."Configurations"
(
	"Code" varchar (100),
	
	constraint "Configurations_PK" primary key ("Code"),
	
	"Value" varchar (500) not null,

	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedOn" timestamptz
);

-- the configuration are stored on the specific environment scripts

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
	('LDAPHost'),
	('LDAPPort'),
	('SmtpHost'),
	('SmtpPort'),
	('SmtpUsername'),
	('SmtpPassword'),
	('EmailSender'),
	('UserBlockMessage'),
	('UserBlockWarningMessage'),
	('SignatureMessage'),
	('InstanceMessage'),
	('NotificationMessage'),
	('DecimalSeparator'),
	('RequireUserReason'),
	('PrototypesAttachmentMaxSizeBytes'),
	('InstancesAttachmentMaxSizeBytes'),
	('InactivityTimeoutSeconds'),
	('MaximumNumberOfPasswordFailure'),
	('MaximumNumberOfPINFailure'),
	('PINDaysLife'),
	('WarningUserInactivityDays'),
	('MaximumUserInactivityDays'),
	('LicenseExpiration'),
	('LicensePrototypeTypesCount'),
	('LicensePrototypesCount'),
	('LicenseUsersCount'),
	('LicenseSignature');

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

create table "Configuration"."UserSettings"
(
	"Code" varchar (100) primary key
);

insert into "Configuration"."UserSettings" ("Code") values
	('GridSetup');

create table "Configuration"."UserSettingValues"
(
	"Company" bigint not null,
	"User" varchar (60),
	constraint "UserSessions_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Setting" varchar (100) not null references "Configuration"."UserSettings",
	"Code" varchar(100) not null,

	constraint "UserSettings_PK" primary key ("Company", "User", "Setting", "Code"),

	"Value" text not null,

	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedOn" timestamptz
);

create unique index "Index_UserSettingValues_ID" on "Configuration"."SettingValues" ("Company", "Setting");