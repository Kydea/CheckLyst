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
	(1, 'CheckListManager', 'Companies', 'View');

insert into "Trail"."Actions"
values 
	('Company created'),
	('Company updated'),
	('Company deleted');

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

