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
drop index "Security"."Index_UserFunctions_AppointedUsers";
create unique index "Index_UserFunctions_Function" on "Security"."UserFunctions" ("Company", "User", "Department", "PrototypeType", "FunctionGroup", "Function");
create index "Index_UserFunctions_AppointedUsers" on "Security"."UserFunctions" ("Company", "FunctionGroup", "Function", "Department", "PrototypeType");

create table "Security"."Roles"
(
    "Company" bigint references "Infrastructure"."Companies",
	"Code" varchar (60),
    
    constraint "Roles_PK" primary key ("Company", "Code"),
    
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
)

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

create unique index "Index_GroupFunctions_Function" on "Security"."GroupFunctions" ("Company", "Group", "FunctionGroup", "Function", "Department", "PrototypeType");	

TODO:
--fare script creazione gruppi e porting di tag
alter table "Security"."UserFunctions" drop column "Tag";