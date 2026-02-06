alter table "Infrastructure"."Companies" add "Online" boolean not null default true;
alter table "Security"."Users" add "TimeZone" varchar (100) references "Infrastructure"."TimeZones";
alter table "CheckLists"."InstanceExpressionValues" add "Task" boolean not null default false;
alter table "Security"."Users" add "PrototypeWorkFlowLevel" varchar (50);

insert into "Configuration"."Configurations" values ('ContextualReportTimeout', '60');

insert into "Configuration"."Settings" values ('LicenseExpiration');
insert into "Configuration"."Settings" values ('LicensePrototypeTypesCount');
insert into "Configuration"."Settings" values ('LicensePrototypesCount');
insert into "Configuration"."Settings" values ('LicenseUsersCount');
insert into "Configuration"."Settings" values ('LicenseSignature');

create index "Index_InstanceExpressionValues_Tasks" on "CheckLists"."InstanceExpressionValues"("Value", "Instance", "Code") where not "Task" is null;

drop index "Reports"."ReportClauses_Reports";
create index "ReportClauses_Report" on "Reports"."ReportClauses" ("Report");

create table "Reports"."ReportRuns"
(
	"ID" bigserial primary key,

	"Company" bigint not null,
	"User" varchar (60) not null,
	constraint "ReportRuns_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Report" bigint not null references "Reports"."Reports" on delete cascade,

	"Expiration" timestamptz not null,
	"Parameters" text,
	"Format" varchar (50),
	"Data" bytea,
	"Error" text,

	"StartedOn" timestamptz,
	"CompletedOn" timestamptz,

	"CreatedOn" timestamptz not null default current_timestamp
);

create index "ReportRuns_User" on "Reports"."ReportRuns" ("Company", "User");

create table "Reports"."UserReports"
(
	"Company" bigint,
	"User" varchar (60),
	constraint "UserReports_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"ReportCode" varchar(50),

	constraint "UserReports_PK" primary key ("Company", "User", "ReportCode")
);

create table "Reports"."ReportStates"
(
	"Code" varchar (20) primary key
);

insert into "Reports"."ReportStates" 
values 
	('Draft'),
	('Completed'),
	('Confirmed'),
	('Obsolete');

alter table "Reports"."Reports" add "Version" int not null default 1;
alter table "Reports"."Reports" add "ReportState" varchar (20) not null references "Reports"."ReportStates" default 'Draft';
alter table "Reports"."Reports" add "ValidFrom" timestamptz;
alter table "Reports"."Reports" add "ValidUntil" timestamptz;
alter table "Reports"."Reports" add "ProposedState" varchar (20) references "Reports"."ReportStates";
alter table "Reports"."Reports" add "ProposedDate" timestamptz;
alter table "Reports"."Reports" add "WorkFlow" bigint references "WorkFlows"."WorkFlows";
alter table "Reports"."Reports" add "Note" text;

update "Reports"."Reports" set "ReportState" = 'Confirmed' where "Online";
alter table "Reports"."Reports" drop column "Online";

drop index "Reports"."Reports_Code";
create unique index "Reports_Code_Version" on "Reports"."Reports" ("Company", upper("Code"), "Version");
create unique index "Reports_ActiveVersion" on "Reports"."Reports" ("Company", upper("Code"), "ReportState") where "ReportState" in ('Confirmed');
create unique index "Reports_IdEditVersion" on "Reports"."Reports" ("Company", upper("Code"), "ReportState") where "ReportState" in ('Draft', 'Completed');
create index "Reports_Code_Version_2" on "Reports"."Reports" ("Company", "Code", "Version");

alter table "WorkFlows"."WorkFlowApplications" add "Report" bigint;
alter table "WorkFlows"."WorkFlowApplications" add constraint "WorkFlowApplications_Reports" foreign key ("Report") references "Reports"."Reports" on delete cascade;
alter table "WorkFlows"."WorkFlowApplications" drop constraint "WorkFlowApplications_check";
alter table "WorkFlows"."WorkFlowApplications" add constraint "Check_WorkFlowApplications_Target" check
(
	not "Prototype" is null and "Instance" is null and "Report" is null
	or "Prototype" is null and not "Instance" is null and "Report" is null
	or "Prototype" is null and "Instance" is null and not "Report" is null
);
insert into "WorkFlows"."WorkFlowActions" values ('Report – state to Confirmed');
insert into "WorkFlows"."WorkFlowActions" values ('Report – state to Obsolete');

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

insert into "Trail"."Actions" values ('Report state changed');