create schema "Settings";

go

create table "Settings"."CompanySettings"
(
	"Company" bigint references "Infrastructure"."Companies" on delete cascade,
	"SettingsGroup" varchar (50),
	"Code" varchar (50),
    
    constraint "CompanySettings_PK" primary key ("Company", "Company", "Code"),

	"Value" text,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);
