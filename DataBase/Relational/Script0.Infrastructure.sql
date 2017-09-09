create schema "Infrastructure" owner to postgres;

create table "Infrastructure"."TimeZones"
(
    "Code" varchar (100) primary key,
    "MicrosoftCode" varchar (100) not null
);

insert into "Infrastructure"."TimeZones" values ('Europe/Rome', 'W. Europe Standard Time');

create table "Infrastructure"."Companies"
(
    "ID" bigserial primary key,
    "Name" varchar (50) not null,
	"TimeZone" varchar (100) not null references "Infrastructure"."TimeZones",

	"SessionTimeout" int not null default 600 constraint "Invalid session timeout" check("SessionTimeout" > 60),
	"ActivityTimeout" int not null default 600 constraint "Invalid activity timeout" check("ActivityTimeout" > 60),

	"MaximumNumberOfPINFailure" int not null default 0,
	"PINDaysLife" int not null default 0,
	"MaximumNumberOfPasswordFailure" int not null default 0,
	"PasswordDaysLife" int not null default 0,
  	"WarningUserInactivityDays" int not null default 0,
  	"MaximumUserInactivityDays" int not null default 0,

	"AuthenticationServer" text,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create table "Infrastructure"."Departments"
(
    "ID" bigserial primary key,
    "Name" varchar (50) not null,

	"Company" bigint references "Infrastructure"."Companies" on delete cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);