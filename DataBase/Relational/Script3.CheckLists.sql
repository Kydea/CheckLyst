create schema "CheckLists";

create table "CheckLists"."PrototypeTypes"
(
	"ID" bigserial primary key,
	"Name" varchar (100) not null,

	"Company" bigint not null references "Infrastructure"."Companies",
	"Department" bigint not null references "Infrastructure"."Departments",
    
	"ApprovationWorkFlow" bigint references "WorkFlows"."WorkFlows",
	"ObsolescenceWorkFlow" bigint references "WorkFlows"."WorkFlows",

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create table "CheckLists"."Prototypes"
(
	"ID" bigserial primary key,
	"Name" varchar (50) not null,
	"Description" varchar (100),

	"Company" bigint not null references "Infrastructure"."Companies",
	"Department" bigint not null references "Infrastructure"."Departments",
	"PrototypeType" bigint not null references "CheckLists"."PrototypeTypes",

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_Prototypes_Name" on "CheckLists"."Prototypes" ("Company", "Name");

create table "CheckLists"."VersionStates"
(
	"Code" varchar (20) primary key
);

insert into "CheckLists"."VersionStates" 
values 
	('Draft'),
	('In simulation'),
	('Approved'),
	('Active'),
	('Obsolete');

create table "CheckLists"."PrototypeVersions"
(
	"Prototype" bigint references "CheckLists"."Prototypes",
	"Version" int,
	constraint "PK_Versions" primary key ("Prototype", "Version"),

	"Description" varchar (100),

	"VersionState" varchar (20) not null references "CheckLists"."VersionStates",
	"ValidFrom" timestamptz,
	"ValidUntil" timestamptz,

	"ProposedState" varchar (20) references "CheckLists"."VersionStates",
	"ProposedDate" timestamptz,

	"InstanceConfirmationWorkFlow" bigint references "WorkFlows"."WorkFlows",	

	"Data" text, --replace with no sql table

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_PrototypeVersions_Drafts" on "CheckLists"."PrototypeVersions"("Prototype", "VersionState") where "VersionState" in ('Draft', 'In simulation', 'Active');

create table "CheckLists"."PrototypeVersionTags"
(
	"Prototype" bigint,
	"Version" int,
	constraint "FK_PrototypeVersionTags_Version" foreign key ("Prototype", "Version") references "CheckLists"."PrototypeVersions" on delete cascade,

	"Code" varchar (50),
	constraint "PK_PrototypeVersionTags" primary key ("Prototype", "Version", "Code")
);

create table "CheckLists"."InstanceNumerators"
(
	"Prototype" bigint not null,
	"Version" int not null,
	constraint "FK_Instances_Version" foreign key ("Prototype", "Version") references "CheckLists"."PrototypeVersions" on delete cascade,

	"Year" int,
	constraint "PK_InstanceNumerators" primary key ("Prototype", "Version", "Year"),

	"Number" int not null default 0	
);

create or replace function "CheckLists"."GetInstanceNumber"
(
	_Prototype bigint,
	_Version int,
	_Year int
) 
returns int as
$$ 
declare
	_Number int;
begin
	_Number = 1;

	select coalesce(n."Number", 0) + 1 into _Number
	from 
		"CheckLists"."InstanceNumerators" n 
	where
		n."Prototype" = _Prototype
		and n."Version" = _Version
		and n."Year" = _Year;

	_Number = coalesce(_Number, 1);

	if _Number = 1 then
		insert into "CheckLists"."InstanceNumerators"("Prototype", "Version", "Year", "Number")
		values (_Prototype, _Version, _Year, _Number);
	else
		update "CheckLists"."InstanceNumerators"
		set "Number" = _Number
		where
			"Prototype" = _Prototype
			and "Version" = _Version
			and "Year" = _Year;
	end if;
    
    return _Number;
end 
$$ language plpgsql;

create table "CheckLists"."InstanceStates"
(
	"Code" varchar (20) primary key
);

insert into "CheckLists"."InstanceStates" 
values 
	('In progress'),
	('Completed'),
	('Confirmed'),
	('Canceled');
	
create table "CheckLists"."Instances"
(
	"ID" bigserial primary key,
	"Code" varchar (50) not null,
    
	"Company" bigint not null references "Infrastructure"."Companies",

	"Prototype" bigint not null references "CheckLists"."Prototypes",
	"Version" int not null,
	constraint "FK_Instances_PrototypeVersions" foreign key ("Prototype", "Version") references "CheckLists"."PrototypeVersions" on delete cascade,
	"PrototypeName" varchar (50) not null,
	"Year" int not null,
	constraint "FK_Instances_InstanceNumerator" foreign key ("Prototype", "Version", "Year") references "CheckLists"."InstanceNumerators",
	"Number" int not null,

	"InstanceState" varchar (20) not null references "CheckLists"."InstanceStates",

	"Description" text,
	"Data" text, --replace with no sql table

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_Instance_Number" on "CheckLists"."Instances" ("Prototype", "Version", "Year", "Number");
create unique index "Index_Instance_PrototypeName" on "CheckLists"."Instances" ("Company", "PrototypeName", "Version", "Year", "Number");
create unique index "Index_Instance_Code" on "CheckLists"."Instances" ("Company", "Code");

create table "CheckLists"."InstanceTags"
(
	"ID" bigserial primary key,
	"Instance" bigint not null,
	constraint "FK_InstanceTags_Instance" foreign key ("Instance") references "CheckLists"."Instances" on delete cascade,
	"Tag" varchar (50) not null,
	"Value" text not null
);

create index "Index_InstanceTags_Tag" on "CheckLists"."InstanceTags"("Instance", "Tag");

create table "CheckLists"."InstanceExpressionValues"
(
	"Instance" bigint not null references "CheckLists"."Instances",
	"Code" varchar (50),

	constraint "PK_InstanceExpressionValues" primary key ("Instance", "Code"),

	"Value" text not null
);

create table "CheckLists"."PrototypeVersionsHistory"
(
	"ID" bigserial primary key,

	"Prototype" bigint not null,
    "Version" int not null,
    constraint "PrototypeVersionsHistory_PrototypeVersion" foreign key ("Prototype", "Version") references "CheckLists"."PrototypeVersions" on delete cascade,

	"Date" timestamptz not null default current_timestamp,

	"State" varchar (100) not null,
	"User" varchar (60) not null,

	"WorkFlowApplication" bigint references "WorkFlows"."WorkFlowApplications"
);

create index "Index_PrototypeVersionsHistory_Prototype" on "CheckLists"."PrototypeVersionsHistory"("Prototype", "Date");

create table "CheckLists"."InstancesHistory"
(
	"ID" bigserial primary key,

	"Instance" bigint not null references "CheckLists"."Instances" on delete cascade,

	"Date" timestamptz not null default current_timestamp,

	"State" varchar (100) not null,
	"User" varchar (60) not null,

	"WorkFlowApplication" bigint references "WorkFlows"."WorkFlowApplications"
);

create index "Index_InstancesHistory_Instance" on "CheckLists"."InstancesHistory"("Instance", "Date");