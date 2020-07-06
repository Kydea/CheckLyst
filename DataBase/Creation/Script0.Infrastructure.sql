create schema "Infrastructure";

create table "Infrastructure"."TimeZones"
(
    "Code" varchar (100) primary key,
);

insert into "Infrastructure"."TimeZones" values 
	('Atlantic Standard Time'),
	('E. South America Standard Time'),
	('SA Western Standard Time'),
	('W. Europe Standard Time'),
    ('Mexico Standard Time'); 

create table "Infrastructure"."Companies"
(
    "ID" bigserial primary key,
    "Name" varchar (50) not null,
	"TimeZone" varchar (100) not null references "Infrastructure"."TimeZones",

	"SessionTimeout" int not null default 600 constraint "Invalid session timeout" check("SessionTimeout" > 60),
	"ActivityTimeout" int not null default 600 constraint "Invalid activity timeout" check("ActivityTimeout" > 60),

	"MaximumNumberOfPINFailure" int not null default 0,
	"LoginFailedTimeout" int not null default 20,
	"PINDaysLife" int not null default 0,
	"MaximumNumberOfPasswordFailure" int not null default 0,
	"PasswordDaysLife" int not null default 0,
  	"WarningUserInactivityDays" int not null default 0,
  	"MaximumUserInactivityDays" int not null default 0,
  	"InactivityTimeoutSeconds" int not null default 900,

	"AuthenticationServer" text,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

insert into "Infrastructure"."Companies"("Name", "TimeZone") values ('(CheckListManager)', 'Europe/Rome');

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

create unique index "Departments_Name" on "Infrastructure"."Departments" ("Company", upper("Name"));

create table "Infrastructure"."FTPServers"
(
    "ID" bigserial primary key,

	"Company" bigint references "Infrastructure"."Companies" on delete cascade,

    "Name" varchar (50) not null,
    "Protocol" varchar(50) not null,
    "Host" varchar(500) not null,
    "Port" int not null default 21,
    "Folder" varchar(500),
    "UserName" varchar(100),
    "Password" varchar(100),
    "Passive" boolean not null, 
    "SSL" boolean not null, 
    "Key" text,
    "PrototypesDefault" boolean not null, 
    "InstancesDefault" boolean not null, 

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "FTPServers_Name" on "Infrastructure"."FTPServers" ("Company", upper("Name"));
create unique index "FTPServers_PrototypesDefault" on "Infrastructure"."FTPServers" ("Company") where "PrototypesDefault";
create unique index "FTPServers_InstancesDefault" on "Infrastructure"."FTPServers" ("Company") where "InstancesDefault";