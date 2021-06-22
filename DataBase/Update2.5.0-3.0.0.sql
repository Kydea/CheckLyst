create unique index "Companies_Name" on "Infrastructure"."Companies" (upper("Name"))

alter table "Trail"."Records" add column "CheckListID" bigint;
alter table "Trail"."Records" rename column "CheckList" to "CheckListCode";

drop index "Trail"."Index_Records_CheckList";
create index "Index_Records_CheckListCode" on "Trail"."Records" ("Company", "CheckListCode");
create index "Index_Records_CheckListID" on "Trail"."Records" ("Company", "CheckListID");

insert into "Trail"."Actions" values ('Instance external source row imported');
alter table "Security"."Users" add column "CorporateID" varchar(50);

create table "CheckLists"."InstanceRelations"
(
	"Parent" bigint references "CheckLists"."Instances" on delete cascade,
	"Item" int,
	constraint "PK_InstanceRelations" primary key ("Parent", "Item"),

	"Child" bigint references "CheckLists"."Instances" on delete cascade,
	constraint "InstanceRelations_SelfReference" check ("Parent" <> "Child")
);

create index "Index_InstanceRelations_Child" on "CheckLists"."InstanceRelations"("Child");

alter table "CheckLists"."InstanceExpressionValues" alter column "NotificationNote" type varchar (200);
alter table "CheckLists"."InstanceExpressionValues" alter column "NotificationEmail" type varchar (200);

insert into "Security"."FunctionGroups" values
	('Companies'),
	('Reports');

insert into "Security"."Functions" values
	('Companies', 'Edit'),
	('Companies', 'View'),
	('Reports', 'Edit'),
	('Reports', 'View');

update "Security"."Users" set "Enabled" = true where "Company" = 1 and "Code" = 'CheckListManager';
insert into "Security"."UserFunctions"("Company", "User", "FunctionGroup", "Function") values 
	(1, 'CheckListManager', 'Companies', 'Edit'),
	(1, 'CheckListManager', 'Companies', 'View'),
	(1, 'CheckListManager', 'Users', 'Edit'),
	(1, 'CheckListManager', 'Users', 'View'),
	(1, 'CheckListManager', 'Trail', 'View');

insert into "Trail"."Actions"
values 
	('Group created'),
	('Group edited'),
	('Group deleted'),
	('Role created'),
	('Role edited'),
	('Role deleted'),
	('Company created'),
	('Company updated'),
	('Report created'),
	('Report updated'),
	('Report deleted');

insert into "Configuration"."Settings" ("Code") values
	('InactivityTimeoutSeconds'),
	('MaximumNumberOfPasswordFailure'),
	('MaximumNumberOfPINFailure'),
	('PINDaysLife'),
	('WarningUserInactivityDays'),
	('MaximumUserInactivityDays');

insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") select "ID", 'InactivityTimeoutSeconds', "InactivityTimeoutSeconds" from "Infrastructure"."Companies" order by "ID";
insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") select "ID", 'MaximumNumberOfPasswordFailure', "MaximumNumberOfPasswordFailure" from "Infrastructure"."Companies" order by "ID";
insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") select "ID", 'MaximumNumberOfPINFailure', "MaximumNumberOfPINFailure" from "Infrastructure"."Companies" order by "ID";
insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") select "ID", 'PINDaysLife', "PINDaysLife" from "Infrastructure"."Companies" order by "ID";
insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") select "ID", 'WarningUserInactivityDays', "WarningUserInactivityDays" from "Infrastructure"."Companies" order by "ID";
insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") select "ID", 'MaximumUserInactivityDays', "MaximumUserInactivityDays" from "Infrastructure"."Companies" order by "ID";

alter table "Infrastructure"."Companies" drop column "InactivityTimeoutSeconds";
alter table "Infrastructure"."Companies" drop column "MaximumNumberOfPINFailure";
alter table "Infrastructure"."Companies" drop column "PINDaysLife";
alter table "Infrastructure"."Companies" drop column "WarningUserInactivityDays";
alter table "Infrastructure"."Companies" drop column "MaximumUserInactivityDays";
alter table "Infrastructure"."Companies" drop column "SessionTimeout";
alter table "Infrastructure"."Companies" drop column "ActivityTimeout";
alter table "Infrastructure"."Companies" drop column "LoginFailedTimeout";
alter table "Infrastructure"."Companies" drop column "MaximumNumberOfPasswordFailure";
alter table "Infrastructure"."Companies" drop column "PasswordDaysLife";
alter table "Infrastructure"."Companies" drop column "AuthenticationServer";

delete from "Configuration"."SettingValues" where "Setting" in ('InstancesFtpConnection', 'PrototypesFtpConnection');
delete from "Configuration"."Settings" where "Code" in ('InstancesFtpConnection', 'PrototypesFtpConnection');

drop index "Security"."Index_UserFunctions_Function";
drop index "Security"."Index_UserFunctions_AppointedUsers";
create unique index "Index_UserFunctions_Function" on "Security"."UserFunctions" ("Company", "User", "Department", "PrototypeType", "FunctionGroup", "Function");

create table "Security"."UserLevels"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"User" varchar (60) not null,
    constraint "UserFunctions_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"SignatureLevel" varchar (50),
	"PrototypeWorkFlowLevel" varchar (50),
	"InstanceWorkFlowLevel" varchar (50),

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_UserLevels_Level" on "Security"."UserLevels" ("Company", "User", "Department", "PrototypeType");

create table "Security"."Roles"
(
    "Company" bigint references "Infrastructure"."Companies",
	"Code" varchar (60),
    
    constraint "Roles_PK" primary key ("Company", "Code"),

	"FunctionGroup" varchar (20) references "Security"."FunctionGroups", --Prototypes, Instances o null
    
	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Roles_Code" on "Security"."Roles" ("Company", upper("Code"));

create table "Security"."RoleFunctions"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"Role" varchar (60) not null,
    constraint "RoleFunctions_Role" foreign key ("Company", "Role") references "Security"."Roles" on update cascade on delete cascade,

	"FunctionGroup" varchar (20) not null,
	"Function" varchar (20) not null,
	constraint "RoleFunctions_Function" foreign key ("FunctionGroup", "Function") references "Security"."Functions" on update cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_RoleFunctions_Function" on "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function");

create table "Security"."UserRoles"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"User" varchar (60) not null,
    constraint "UserRoles_Group" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"Role" varchar (60) not null,
    constraint "UserRoles_Role" foreign key ("Company", "Role") references "Security"."Roles" on update cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create index "Index_UserRoles_Role" on "Security"."UserRoles" ("Company", "User", "Department", "PrototypeType");

create table "Security"."Groups"
(
    "Company" bigint references "Infrastructure"."Companies",
	"Code" varchar (60),
    
    constraint "Groups_PK" primary key ("Company", "Code"),
    
	"Description" varchar (100) not null,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Groups_Code" on "Security"."Groups" ("Company", upper("Code"));

create table "Security"."GroupRoles"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"Group" varchar (60) not null,
    constraint "GroupRoles_Group" foreign key ("Company", "Group") references "Security"."Groups" on update cascade on delete cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"Role" varchar (60) not null,
	constraint "GroupRoles_Role" foreign key ("Company", "Role") references "Security"."Roles" on update cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create index "Index_GroupRoles_Role" on "Security"."GroupRoles" ("Company", "Group", "Department", "PrototypeType");

create table "Security"."GroupFunctions"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"Group" varchar (60) not null,
    constraint "GroupFunctions_Group" foreign key ("Company", "Group") references "Security"."Groups" on update cascade on delete cascade,

	"FunctionGroup" varchar (20) not null,
	"Function" varchar (20) not null,
	constraint "GroupFunctions_Function" foreign key ("FunctionGroup", "Function") references "Security"."Functions" on update cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_GroupFunctions_Function" on "Security"."GroupFunctions" ("Company", "Group", "Department", "PrototypeType", "FunctionGroup", "Function");

create table "Security"."UserGroups"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"User" varchar (60) not null,
    constraint "UserRoles_Group" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Group" varchar (60) not null,
    constraint "UserGroups_Group" foreign key ("Company", "Group") references "Security"."Groups" on update cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_UserGroups_Group" on "Security"."UserGroups" ("Company", "Group", "User");

insert into "Security"."UserLevels" ("Company", "User", "Department", "PrototypeType", "SignatureLevel", "PrototypeWorkFlowLevel", "InstanceWorkFlowLevel") 
select 
	uf."Company", uf."User", uf."Department", uf."PrototypeType", 
	max (case when "FunctionGroup" = 'Instances' then uf."Tag" else null end), 
	max (case when "FunctionGroup" = 'Prototypes' then uf."Tag" else null end),
	max (case when "FunctionGroup" = 'Instances' then uf."Tag" else null end)
from "Security"."UserFunctions" uf
where 
	"FunctionGroup" in ('Prototypes', 'Instances')
	and not uf."Tag" is null
group by uf."Company", uf."User", uf."Department", uf."PrototypeType"
order by uf."Company", uf."User", uf."Department", uf."PrototypeType";

insert into "Security"."Roles" ("Company", "Code") select "ID", 'Local Security Administrator' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code") select "ID", 'Local System Administrator' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code") select "ID", 'Global System Administrator' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code") select "ID", 'System Support' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code", "FunctionGroup") select "ID", 'Prototype Viewer', 'Prototypes' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code", "FunctionGroup") select "ID", 'Prototype Configurator', 'Prototypes' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code", "FunctionGroup") select "ID", 'Prototype Simulator', 'Prototypes' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code", "FunctionGroup") select "ID", 'Prototype Approver', 'Prototypes' from "Infrastructure"."Companies" order by "ID"; 
insert into "Security"."Roles" ("Company", "Code", "FunctionGroup") select "ID", 'Instance Viewer', 'Instances' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code", "FunctionGroup") select "ID", 'Instance Executor', 'Instances' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code", "FunctionGroup") select "ID", 'Instance Supervisor', 'Instances' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."Roles" ("Company", "Code", "FunctionGroup") select "ID", 'Instance Approver', 'Instances' from "Infrastructure"."Companies" order by "ID"; 

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local Security Administrator', 'Trail', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local Security Administrator', 'Users', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local Security Administrator', 'Users', 'Edit' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'DataSources', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'DataSources', 'Edit' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'Departments', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'Departments', 'Edit' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'PrototypeTypes', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'PrototypeTypes', 'Edit' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'Trail', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'Users', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'Users', 'Edit' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'WorkFlows', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'WorkFlows', 'Edit' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'FTPServers', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Global System Administrator', 'FTPServers', 'Edit' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local System Administrator', 'DataSources', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local System Administrator', 'Departments', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local System Administrator', 'Departments', 'Edit' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local System Administrator', 'PrototypeTypes', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local System Administrator', 'Trail', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local System Administrator', 'Users', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local System Administrator', 'WorkFlows', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local System Administrator', 'WorkFlows', 'Edit' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Local System Administrator', 'FTPServers', 'View' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'System Support', 'DataSources', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'System Support', 'Departments', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'System Support', 'PrototypeTypes', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'System Support', 'Trail', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'System Support', 'Users', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'System Support', 'WorkFlows', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'System Support', 'FTPServers', 'View' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Prototype Viewer', 'Prototypes', 'View' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Prototype Simulator', 'Prototypes', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Prototype Simulator', 'Prototypes', 'Preview' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Prototype Configurator', 'Prototypes', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Prototype Configurator', 'Prototypes', 'Preview' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Prototype Configurator', 'Prototypes', 'Edit' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Prototype Approver', 'Prototypes', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Prototype Approver', 'Prototypes', 'Preview' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Prototype Approver', 'Prototypes', 'Approve' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Viewer', 'Instances', 'View' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Executor', 'Instances', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Executor', 'Instances', 'Edit' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Executor', 'Instances', 'Create' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Executor', 'Instances', 'Confirm' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Supervisor', 'Instances', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Supervisor', 'Instances', 'Edit' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Supervisor', 'Instances', 'Create' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Supervisor', 'Instances', 'Confirm' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Supervisor', 'Instances', 'Delete' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Supervisor', 'Instances', 'Unsign' from "Infrastructure"."Companies" order by "ID";

insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Approver', 'Instances', 'View' from "Infrastructure"."Companies" order by "ID";
insert into "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function") select "ID", 'Instance Approver', 'Instances', 'Confirm' from "Infrastructure"."Companies" order by "ID";

alter table "Security"."UserFunctions" drop column "Tag";


drop index "CheckLists"."PrototypeTypes_Name";
create unique index "PrototypeTypes_Name" on "CheckLists"."PrototypeTypes" ("Department", upper("Name"));

create function "Security"."GetUserGrants"(_Company bigint, _Code varchar(60), _FunctionGroup varchar(20), _Function varchar(20))
returns table ("Department" bigint, "PrototypeType" bigint) as
$func$
																															 
	select uf."Department", uf."PrototypeType"
	from "Security"."UserFunctions" uf
	where
		uf."Company" = _Company
		and uf."User" = _Code
		and uf."FunctionGroup" = _FunctionGroup
		and uf."Function" = _Function
		and not uf."Department" is null
	union
	select gf."Department", gf."PrototypeType"
	from 
		"Security"."UserGroups" ug
		inner join "Security"."GroupFunctions" gf on ug."Company" = gf."Company" and ug."Group" = gf."Group"
	where
		ug."Company" = _Company
		and ug."User" = _Code
		and gf."FunctionGroup" = _FunctionGroup
		and gf."Function" = _Function
		and not gf."Department" is null
	union
	select ur."Department", ur."PrototypeType"
	from 
		"Security"."UserRoles" ur
		inner join "Security"."RoleFunctions" rf on ur."Company" = rf."Company" and ur."Role" = rf."Role"
	where
		ur."Company" = _Company
		and ur."User" = _Code
		and rf."FunctionGroup" = _FunctionGroup
		and rf."Function" = _Function
		and not ur."Department" is null
	union
	select gr."Department", gr."PrototypeType"
	from 
		"Security"."UserGroups" ug
		inner join "Security"."GroupRoles" gr on ug."Company" = gr."Company" and ug."Group" = gr."Group"
		inner join "Security"."RoleFunctions" rf on gr."Company" = rf."Company" and gr."Role" = rf."Role"
	where
		ug."Company" = _Company
		and ug."User" = _Code
		and rf."FunctionGroup" = _FunctionGroup
		and rf."Function" = _Function
		and not gr."Department" is null

$func$ language sql;

create function "Security"."GetAllUserGrants"(_Company bigint, _Code varchar(60))
returns table ("Group" varchar(60), "Department" bigint, "PrototypeType" bigint, "Role" varchar(60), "FunctionGroup" varchar(20), "Function" varchar(20)) as
$func$
																															 
	select null, uf."Department", uf."PrototypeType", null, uf."FunctionGroup", uf."Function"
	from "Security"."UserFunctions" uf
	where
		uf."Company" = _Company
		and uf."User" = _Code
	union
	select ug."Group", gf."Department", gf."PrototypeType", null, gf."FunctionGroup", gf."Function"
	from 
		"Security"."UserGroups" ug
		inner join "Security"."GroupFunctions" gf on ug."Company" = gf."Company" and ug."Group" = gf."Group"
	where
		ug."Company" = _Company
		and ug."User" = _Code
	union
	select null, ur."Department", ur."PrototypeType", ur."Role", null, null
	from 
		"Security"."UserRoles" ur
	where
		ur."Company" = _Company
		and ur."User" = _Code
	union
	select ug."Group", gr."Department", gr."PrototypeType", gr."Role", null, null
	from 
		"Security"."UserGroups" ug
		inner join "Security"."GroupRoles" gr on ug."Company" = gr."Company" and ug."Group" = gr."Group"
	where
		ug."Company" = _Company
		and ug."User" = _Code

$func$ language sql;

create function "Security"."GetGrantedUsers"(_Company bigint, _Department bigint, _PrototypeType bigint, _FunctionGroup varchar(20), _Function varchar(20))
returns setof "Security"."Users" as
$func$
																															 
	select u.*
	from "Security"."Users" u
	where
		u."Company" = _Company
		and (
			exists (
				select 1 
				from "Security"."UserFunctions" uf
				where
					uf."Company" = u."Company"
					and uf."User" = u."Code"
					and uf."FunctionGroup" = _FunctionGroup
					and uf."Function" = _Function
					and uf."Department" = _Department
					and (uf."PrototypeType" is null or uf."PrototypeType" = _PrototypeType)
			)
			or exists (
				select 1
				from 
					"Security"."UserGroups" ug
					inner join "Security"."GroupFunctions" gf on ug."Company" = gf."Company" and ug."Group" = gf."Group"
				where
					ug."Company" = u."Company"
					and ug."User" = u."Code"
					and gf."FunctionGroup" = _FunctionGroup
					and gf."Function" = _Function
					and gf."Department" = _Department
					and (gf."PrototypeType" is null or gf."PrototypeType" = _PrototypeType)
			)
			or exists (
				select 1
				from 
					"Security"."UserRoles" ur
					inner join "Security"."RoleFunctions" rf on ur."Company" = rf."Company" and ur."Role" = rf."Role"
				where
					ur."Company" = u."Company"
					and ur."User" = u."Code"
					and rf."FunctionGroup" = _FunctionGroup
					and rf."Function" = _Function
					and ur."Department" = _Department
					and (ur."PrototypeType" is null or ur."PrototypeType" = _PrototypeType)
			)
			or exists (
				select 1
				from 
					"Security"."UserGroups" ug
					inner join "Security"."GroupRoles" gr on ug."Company" = gr."Company" and ug."Group" = gr."Group"
					inner join "Security"."RoleFunctions" rf on gr."Company" = rf."Company" and gr."Role" = rf."Role"
				where
					ug."Company" = u."Company"
					and ug."User" = u."Code"
					and rf."FunctionGroup" = _FunctionGroup
					and rf."Function" = _Function
					and gr."Department" = _Department
					and (gr."PrototypeType" is null or gr."PrototypeType" = _PrototypeType)
			)
		)
$func$ language sql;

create table "CheckLists"."InstanceData"
(
	"Instance" bigint primary key references "CheckLists"."Instances" on delete cascade,
	"Company" bigint references "Infrastructure"."Companies",
	"ItemsByLabel" jsonb,
	"ItemsByCode" jsonb,
	"Expressions" jsonb
);

create index "Index_ItemsByLabel" on "CheckLists"."InstanceData" using gin ("ItemsByLabel");
create index "Index_ItemsByCode" on "CheckLists"."InstanceData" using gin ("ItemsByCode");
create index "Index_ItemsByExpressions" on "CheckLists"."InstanceData" using gin ("Expressions");

create function "CheckLists"."UpdateInstanceData"
(
	"_Instance" bigint,
	"_Company" bigint,
	"_ItemsByCode" text,
	"_ItemsByLabel" text,
	"_Expressions" text
) 
returns int as
$$ 
declare 
	"_ItemsByCodeJSON" jsonb := "_ItemsByCode"::jsonb;
	"_ItemsByLabelJSON" jsonb := "_ItemsByLabel"::jsonb;
	"_ExpressionsJSON" jsonb := "_Expressions"::jsonb;
begin
	insert into "CheckLists"."InstanceData" ("Instance", "Company", "ItemsByLabel", "ItemsByCode", "Expressions")
	values ("_Instance", "_Company", "_ItemsByCodeJSON", "_ItemsByLabelJSON", "_ExpressionsJSON")
	on conflict ("Instance") do update set
		"ItemsByCode" = "_ItemsByCodeJSON", 
		"ItemsByLabel" = "_ItemsByLabelJSON", 
		"Expressions" = "_ExpressionsJSON";

	return 0;
end
$$
language plpgsql;

create table "Security"."GroupConflicts"
(
    "Company" bigint not null,

	"Group1" varchar (60) not null,
	constraint "GroupConflicts_Group1" foreign key ("Company", "Group1") references "Security"."Groups" on update cascade on delete cascade,

	"Group2" varchar (60) not null,
	constraint "GroupConflicts_Group2" foreign key ("Company", "Group2") references "Security"."Groups" on update cascade on delete cascade,

    constraint "GroupConflicts_PK" primary key ("Company", "Group1", "Group2"),

    constraint "GroupConflicts_SelfConflict" check ("Group1" <> "Group2")
);

create unique index "Index_GroupFunctions_Groups2" on "Security"."GroupConflicts" ("Company", "Group2", "Group1");

insert into "Configuration"."Settings" ("Code") values ('LDAPPort');
insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") select "ID", 'LDAPPort', '636' from "Infrastructure"."Companies" order by "ID";

--Script8.Reports.sql