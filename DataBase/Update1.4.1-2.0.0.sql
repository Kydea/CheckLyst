insert into "Configuration"."Configurations" ("Code", "Value") values
	('WebApplicationUrl', <TBD>), --update Configuration - XXX.sql accordingly, the url must be specified without final slash
	('PrototypesStorageBucket', <TBD>), --update Configuration - XXX.sql accordingly
	('InstancesStorageBucket', <TBD>); --update Configuration - XXX.sql accordingly

alter table "CheckLists"."InstanceExpressionValues" add "Description" varchar (100);
alter table "CheckLists"."InstanceExpressionValues" add	"Unique" boolean not null default true;
alter table "CheckLists"."InstanceExpressionValues" add	"Exception" boolean not null default false;

create or replace function "CheckLists"."ValidateUniqueExpressions"
(
	_Instance bigint
) 
returns int as
$$ 
declare
	_Year varchar(4);
	_Number int;
	_Code varchar(200);
	_Value text;
begin
	_Year = null;
	
	select 
		case when i2."Year" > 0 then cast(i2."Year" as varchar(4)) when pv."VersionState" = 'Draft' then 'TEST' else 'SE' end,
		i2."Number",
		case when iev1."Description" is null then iev1."Code" else iev1."Description" || ' (' || iev1."Code" || ')' end, iev1."Value"
	into _Year, _Number, _Code, _Value
	from
		"CheckLists"."Instances" i1
		inner join "CheckLists"."PrototypeVersions" pv on i1."Prototype" = pv."Prototype" and i1."Version" = pv."Version"
		inner join "CheckLists"."Instances" i2 on i1."Prototype" = i2."Prototype" and i1."Version" = i2."Version" and i2."InstanceState" = 'In progress' and i1."ID" <> i2."ID"
		inner join "CheckLists"."InstanceExpressionValues" iev1 on i1."ID" = iev1."Instance" and iev1."Unique"
		inner join "CheckLists"."InstanceExpressionValues" iev2 on i2."ID" = iev2."Instance" and iev1."Code" = iev2."Code" and iev1."Value" = iev2."Value" and iev1."Unique"
	where
		i1."ID" = _Instance
	limit 1;
	
	if not _Year is null then
		raise 'The value % for the expression % has been found in the in progress instance % - %', _Value, _Code, _Year, _Number;
	end if;
																	   
	return 0;
end
$$
language plpgsql;

alter table "WorkFlows"."WorkFlowApplicationSteps" add "UserDescription" varchar (100);

update "WorkFlows"."WorkFlowApplicationSteps"
set 
	"UserDescription" = (select u."Description" from "Security"."Users" u where u."Company" = "WorkFlowApplicationSteps"."Company" and u."Code" = "WorkFlowApplicationSteps"."User")
where 
	not "User" is null;

alter table "Infrastructure"."Companies" add "InactivityTimeoutSeconds" int not null default 900;

create table "Infrastructure"."FTPServers"
(
    "ID" bigserial primary key,

	"Company" bigint references "Infrastructure"."Companies" on delete cascade,

    "Name" varchar (50) not null,
    "Host" varchar(500) not null,
    "Port" int not null default 21,
    "Folder" varchar(500),
    "UserName" varchar(100),
    "Password" varchar(100),
    "Passive" boolean not null, 
    "SSL" boolean not null, 
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

insert into "Security"."FunctionGroups" values
	('FTPServers');

insert into "Security"."Functions" values
	('FTPServers', 'Edit'),
	('FTPServers', 'View');


insert into "Configuration"."Settings" ("Code") values
	('InstanceMessage');

insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") 
select "ID", 'InstanceMessage', '[User], <br /> A signature has been issued for the instance [CheckListCode].'
from "Infrastructure"."Companies";


alter table "CheckLists"."PrototypeTypes" add "PrototypesFTPServerEnabled" boolean not null default true;
alter table "CheckLists"."PrototypeTypes" add "PrototypesFTPServer" bigint references "Infrastructure"."FTPServers";
alter table "CheckLists"."PrototypeTypes" add "InstancesFTPServerEnabled" boolean not null default false;
alter table "CheckLists"."PrototypeTypes" add "InstancesFTPServer" bigint references "Infrastructure"."FTPServers";

insert into "Security"."Functions" values
	('Prototypes', 'Preview');

insert into "Security"."UserFunctions" ("Company", "User", "FunctionGroup", "Function", "Department", "PrototypeType", "Tag") 
select "Company", "User", 'Prototypes', 'Preview', "Department", "PrototypeType", "Tag"
from "Security"."UserFunctions"
where
	"FunctionGroup" = 'Prototypes' 
	and "Function" = 'Edit';

insert into "Trail"."Actions"
values
	('Email sent');

insert into "Trail"."Actions"
values
	('Instance signature failed');

insert into "Trail"."Actions"
values
	('Workflow signature issued'),
	('Workflow signature failed');
	
insert into "Configuration"."Settings" ("Code") values
	('RequireUserReason');

insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") 
select "ID", 'RequireUserReason', 'true'
from "Infrastructure"."Companies";

insert into "Trail"."Actions"
values
	('Workflow created'),
	('Workflow updated'),
	('Workflow deleted');

insert into "Trail"."Actions"
values
	('DataSource created'),
	('DataSource updated'),
	('DataSource deleted');

insert into "Trail"."Actions"
values
	('PrototypeType created'),
	('PrototypeType updated'),
	('PrototypeType deleted');

insert into "Trail"."Actions"
values
	('Department created'),
	('Department updated'),
	('Department deleted');

insert into "Trail"."Actions"
values
	('FTPServer created'),
	('FTPServer updated'),
	('FTPServer deleted');

insert into "Configuration"."Settings" ("Code") values
	('PrototypesAttachmentMaxSizeBytes'),
	('InstancesAttachmentMaxSizeBytes');

insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") 
select "ID", 'PrototypesAttachmentMaxSizeBytes', '10485760' -- 10MB
from "Infrastructure"."Companies";

insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") 
select "ID", 'InstancesAttachmentMaxSizeBytes', '10485760' -- 10MB
from "Infrastructure"."Companies";

insert into "Trail"."Actions"
values
	('Instance signature cancelation failed');

insert into "Trail"."Actions"
values
	('Instance downloaded'),
	('Prototype downloaded');

insert into "Infrastructure"."TimeZones" values 
	('Atlantic Standard Time', '1'),
	('E. South America Standard Time', '2'),
	('SA Western Standard Time', '3'),
	('W. Europe Standard Time', '4');

update "Infrastructure"."Companies" set "TimeZone" = (select "MicrosoftCode" from "Infrastructure"."TimeZones" where "Code" = "TimeZone");

alter table "Infrastructure"."TimeZones" drop column "MicrosoftCode";

drop index "DataSources"."Index_DataSources_Company_Code";
create unique index "DataSources_Code" on "DataSources"."DataSources" ("Company", "Code");

--Set the values for prototypes and instances web services
/*
  	<system.web>
		...
		<httpRuntime ... maxQueryStringLength="4096" />
		...
	</system.web>
    <security>
      <requestFiltering>
        <requestLimits maxQueryString="4096" />
      </requestFiltering>
    </security>
*/