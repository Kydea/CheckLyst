alter table "Trail"."Records" add column "CheckListID" bigint;
alter table "Trail"."Records" rename column "CheckList" to "CheckListCode";
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
	('Companies');

insert into "Security"."Functions" values
	('Companies', 'Edit'),
	('Companies', 'View');

update "Security"."Users" set "Enabled" = true where "Company" = 1 and "Code" = 'CheckListManager';
insert into "Security"."UserFunctions"("Company", "User", "FunctionGroup", "Function") values 
	(1, 'CheckListManager', 'Companies', 'Edit'),
	(1, 'CheckListManager', 'Companies', 'View'),
	(1, 'CheckListManager', 'Trail', 'View');

insert into "Trail"."Actions"
values 
	('Company created'),
	('Company updated');

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
    constraint "UserRoles_Role" foreign key ("Company", "Role") references "Security"."Roles" on update cascade on delete cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_UserRoles_Role" on "Security"."UserRoles" ("Company", "User", "Department", "PrototypeType");

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
	constraint "GroupRoles_Role" foreign key ("Company", "Role") references "Security"."Roles" on update cascade on delete cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_GroupRoles_Role" on "Security"."GroupRoles" ("Company", "Group", "Department", "PrototypeType");

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
    constraint "UserGroups_Group" foreign key ("Company", "Group") references "Security"."Groups" on update cascade on delete cascade,

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