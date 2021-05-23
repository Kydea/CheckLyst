create schema "Reports";

create table "Reports"."ReportSources"
(
	"Code" varchar (50) primary key,
	"Query" text not null
);

create table "Reports"."Reports"
(
	"ID" bigserial primary key,

	"Company" bigint not null,	
	"Code" varchar (50) not null,
	"Description" varchar (100),
	
	"ReportSource" varchar(50) references "Reports"."ReportSources",
	"Query" text,

	"InstancesRangeRestricted" boolean not null default false,
	"InstancesIDAlias" varchar(100),

	"PrototypesRangeRestricted" boolean not null default false,
	"PrototypesIDAlias" varchar(100),

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Reports_Name" on "Reports"."Reports" ("Company", upper("Code"));

create table "Reports"."ReportColumns"
(
	"ID" bigserial primary key,

	"Report" bigint not null references "Reports"."Reports" on delete cascade,

	"Label" varchar (50) not null,
	"Column" text,
	"Visible" boolean not null default false,
	"Format" varchar (50),
	"GroupColumn" boolean not null default false,
	"OrderColumn" boolean not null default false,
	"ValueAggregation" varchar(50),
	"GroupAggregation" varchar(50)
);

create  index "ReportColumns_Reports" on "Reports"."ReportColumns" ("Report");

create table "Reports"."ReportClauses"
(
	"ID" bigserial primary key,
	"Report" bigint not null references "Reports"."Reports" on delete cascade,

	"Label" varchar (50) not null,
	"Clause" text, --json clause: column + sign + value or clause + parameter 
	"Visible" boolean not null default false,
	"Editable" boolean not null default false
	"ParameterName" varchar(50),
	"ParameterType" varchar(30),
	"ClauseType" varchar(30)
);

create  index "ReportClauses_Reports" on "Reports"."ReportClauses" ("Report");

--delete from "Reports"."ReportSources";
insert into "Reports"."ReportSources" values
(
	'Audit trail',
	'select * from "Trail"."Records"'
);

insert into "Reports"."ReportSources" values
(
	'Prototypes',
	'select * from "CheckLists"."Prototypes"'
);

insert into "Reports"."ReportSources" values
(
	'Instances',
	'select * from "CheckLists"."Instances"'
);

insert into "Reports"."ReportSources" values
(
	'Users',
	'select * from "Security"."Users"'
);
