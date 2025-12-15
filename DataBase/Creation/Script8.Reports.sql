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

	"Online" boolean not null default false,
	"Restricted" boolean not null default false,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Reports_Code" on "Reports"."Reports" ("Company", upper("Code"));

create table "Reports"."ReportGroups"
(
	"Company" bigint not null,
	"Report" bigint references "Reports"."Reports" on delete cascade,
	"Group" varchar(60),
	constraint "ReportGroups_Group" foreign key ("Company", "Group") references "Security"."Groups" on update cascade,

	constraint PK_ReportGroups primary key ("Company", "Report", "Group")
);

create index "ReportGroups_Report" on "Reports"."ReportGroups" ("Report");

create table "Reports"."ReportColumns"
(
	"ID" bigserial primary key,

	"Report" bigint not null references "Reports"."Reports" on delete cascade,

	"Label" varchar (50) not null,
	"Column" text,
	"Visible" boolean not null default false,
	"Format" varchar (50),
	"GroupColumn" boolean not null default false,
	"OrderColumn" int not null default 0,
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

create index "ReportClauses_Report" on "Reports"."ReportClauses" ("Report");

create table "Reports"."ReportRuns"
(
	"ID" bigserial primary key,

	"Company" bigint not null,
	"User" varchar (60),
	constraint "ReportRuns_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Report" bigint not null references "Reports"."Reports" on delete cascade,

	"Expiration" timestamptz not null,
	"Parameters" text,
	"Data" bytea,

	"StartedOn" timestamptz,
	"CompletedOn" timestamptz,

	"CreatedOn" timestamptz not null default current_timestamp
);

create index "ReportRuns_User" on "Reports"."ReportRuns" ("Company", "User");

alter table "Infrastructure"."Companies" enable row level security;
alter table "Infrastructure"."Departments" enable row level security;
alter table "Security"."Users" enable row level security;
alter table "Security"."UserLevels" enable row level security;
alter table "Security"."UserFunctions" enable row level security;
alter table "Security"."UserRoles" enable row level security;
alter table "Security"."UserGroups" enable row level security;
alter table "Security"."Roles" enable row level security;
alter table "Security"."RoleFunctions" enable row level security;
alter table "Security"."Groups" enable row level security;
alter table "Security"."GroupFunctions" enable row level security;
alter table "Security"."GroupRoles" enable row level security;
alter table "Security"."GroupConflicts" enable row level security;
alter table "CheckLists"."PrototypeTypes" enable row level security;
alter table "CheckLists"."Prototypes" enable row level security;
alter table "CheckLists"."PrototypeVersions" enable row level security;
alter table "CheckLists"."Instances" enable row level security;
alter table "CheckLists"."InstanceTags" enable row level security;
alter table "CheckLists"."InstanceExpressionValues" enable row level security;
alter table "CheckLists"."InstanceRelations" enable row level security;
alter table "CheckLists"."PrototypeVersionsHistory" enable row level security;
alter table "CheckLists"."InstancesHistory" enable row level security;
alter table "Trail"."Records" enable row level security;

create role "ReportRole";

grant connect on database "CheckListManager" to "ReportRole";

grant usage on schema "Infrastructure" to "ReportRole";
grant select on table "Infrastructure"."Companies" to "ReportRole";
grant select on table "Infrastructure"."Departments" to "ReportRole";

grant usage on schema "Security" to "ReportRole";
grant select (
	"Company", "Code", "Description", "AuthenticationCode", "Email", "CorporateID", "Enabled", "PasswordChangeDate", "NumberOfPasswordAttempts", 
	"PINChangeDate", "NumberOfPINAttempts", "LastLoginOn", "LastLoginFailedOn", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn"
) on table "Security"."Users" to "ReportRole";
grant select on table "Security"."UserLevels" to "ReportRole";
grant select on table "Security"."UserFunctions" to "ReportRole";
grant select on table "Security"."UserRoles" to "ReportRole";
grant select on table "Security"."UserGroups" to "ReportRole";
grant select on table "Security"."Roles" to "ReportRole";
grant select on table "Security"."RoleFunctions" to "ReportRole";
grant select on table "Security"."Groups" to "ReportRole";
grant select on table "Security"."GroupFunctions" to "ReportRole";
grant select on table "Security"."GroupRoles" to "ReportRole";
grant select on table "Security"."GroupConflicts" to "ReportRole";
grant execute on function "Security"."GetAllUserGrants" (_Company bigint, _Code varchar(60)) to "ReportRole";

grant usage on schema "CheckLists" to "ReportRole";
grant select on table "CheckLists"."PrototypeTypes" to "ReportRole";
grant select on table "CheckLists"."Prototypes" to "ReportRole";
grant select on table "CheckLists"."PrototypeVersions" to "ReportRole";
grant select on table "CheckLists"."Instances" to "ReportRole";
grant select on table "CheckLists"."InstanceTags" to "ReportRole";
grant select on table "CheckLists"."InstanceExpressionValues" to "ReportRole";
grant select on table "CheckLists"."InstanceRelations" to "ReportRole";
grant select on table "CheckLists"."PrototypeVersionsHistory" to "ReportRole";
grant select on table "CheckLists"."InstancesHistory" to "ReportRole";
grant select on table "CheckLists"."InstanceData" to "ReportRole";

grant usage on schema "Trail" to "ReportRole";
grant select on table "Trail"."Records" to "ReportRole";

create role "ReportRole1";
create policy "Company1Companies" on "Infrastructure"."Companies" to "ReportRole1" using (true);
create policy "Company1Departments" on "Infrastructure"."Departments" to "ReportRole1" using (true);
create policy "Company1Users" on "Security"."Users" to "ReportRole1" using (true);
create policy "Company1UserLevels" on "Security"."UserLevels" to "ReportRole1" using (true);
create policy "Company1UserFunctions" on "Security"."UserFunctions" to "ReportRole1" using (true);
create policy "Company1UserRoles" on "Security"."UserRoles" to "ReportRole1" using (true);
create policy "Company1UserGroups" on "Security"."UserGroups" to "ReportRole1" using (true);
create policy "Company1Roles" on "Security"."Roles" to "ReportRole1" using (true);
create policy "Company1RoleFunctions" on "Security"."RoleFunctions" to "ReportRole1" using (true);
create policy "Company1Groups" on "Security"."Groups" to "ReportRole1" using (true);
create policy "Company1GroupFunctions" on "Security"."GroupFunctions" to "ReportRole1" using (true);
create policy "Company1GroupRoles" on "Security"."GroupRoles" to "ReportRole1" using (true);
create policy "Company1GroupConflicts" on "Security"."GroupConflicts" to "ReportRole1" using (true);
create policy "Company1Prototypes" on "CheckLists"."PrototypeTypes" to "ReportRole1" using (true);
create policy "Company1Prototypes" on "CheckLists"."Prototypes" to "ReportRole1" using (true);
create policy "Company1PrototypeVersions" on "CheckLists"."PrototypeVersions" to "ReportRole1" using (true);
create policy "Company1Instances" on "CheckLists"."Instances" to "ReportRole1" using (true);
create policy "Company1InstanceTags" on "CheckLists"."InstanceTags" to "ReportRole1" using (true);
create policy "Company1InstanceExpressionValues" on "CheckLists"."InstanceExpressionValues" to "ReportRole1" using (true);
create policy "Company1InstanceRelations" on "CheckLists"."InstanceRelations" to "ReportRole1" using (true);
create policy "Company1PrototypeVersionsHistory" on "CheckLists"."PrototypeVersionsHistory" to "ReportRole1" using (true);
create policy "Company1InstancesHistory" on "CheckLists"."InstancesHistory" to "ReportRole1" using (true);
create policy "Company1InstanceData" on "CheckLists"."InstanceData" to "ReportRole1" using (true);
create policy "Company1Records" on "Trail"."Records" to "ReportRole1" using (true);
create user "ReportUser1" in role "ReportRole", "ReportRole1";

create or replace function "Security"."CreateReportUser" (company bigint) returns boolean as 
$$
begin
  execute replace
  (
  	$r$
		create role "ReportRole$";

		create policy "Company$Companies" on "Infrastructure"."Companies" to "ReportRole$" using 
		("ID" = $);
		create policy "Company$Departments" on "Infrastructure"."Departments" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$Users" on "Security"."Users" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$UserLevels" on "Security"."UserLevels" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$UserFunctions" on "Security"."UserFunctions" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$UserRoles" on "Security"."UserRoles" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$UserGroups" on "Security"."UserGroups" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$Roles" on "Security"."Roles" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$RoleFunctions" on "Security"."RoleFunctions" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$Groups" on "Security"."Groups" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$GroupFunctions" on "Security"."GroupFunctions" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$GroupRoles" on "Security"."GroupRoles" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$GroupConflicts" on "Security"."GroupConflicts" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$Prototypes" on "CheckLists"."PrototypeTypes" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$Prototypes" on "CheckLists"."Prototypes" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$PrototypeVersions" on "CheckLists"."PrototypeVersions" to "ReportRole$" using 
		("Prototype" in (select p."ID" from "CheckLists"."Prototypes" p where p."Company" = $));
		create policy "Company$Instances" on "CheckLists"."Instances" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$InstanceTags" on "CheckLists"."InstanceTags" to "ReportRole$" using 
		("Instance" in (select i."ID" from "CheckLists"."Instances" i where i."Company" = $));
		create policy "Company$InstanceExpressionValues" on "CheckLists"."InstanceExpressionValues" to "ReportRole$" using 
		("Instance" in (select i."ID" from "CheckLists"."Instances" i where i."Company" = $));
		create policy "Company$InstanceRelations" on "CheckLists"."InstanceRelations" to "ReportRole$" using 
		("Parent" in (select i."ID" from "CheckLists"."Instances" i where i."Company" = $));
		create policy "Company$PrototypeVersionsHistory" on "CheckLists"."PrototypeVersionsHistory" to "ReportRole$" using 
		("Prototype" in (select p."ID" from "CheckLists"."Prototypes" p where p."Company" = $));
		create policy "Company$InstancesHistory" on "CheckLists"."InstancesHistory" to "ReportRole$" using 
		("Instance" in (select i."ID" from "CheckLists"."Instances" i where i."Company" = $));
		create policy "Company$InstanceData" on "CheckLists"."InstanceData" to "ReportRole$" using 
		("Company" = $);
		create policy "Company$Records" on "Trail"."Records" to "ReportRole$" using 
		("Company" = $);

		create user "ReportUser$" in role "ReportRole", "ReportRole$";
  	$r$, 
  	'$', company::varchar
  );
  
  return true;
end;
$$ language plpgsql;