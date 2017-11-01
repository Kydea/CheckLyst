create schema "CheckLists";

create table "CheckLists"."PrototypeTypes"
(
	"ID" bigserial primary key,
	"Description" varchar (100),

	"Company" bigint references "Infrastructure"."Companies",
	"Department" bigint references "Infrastructure"."Departments",
    
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
	"Name" varchar (50),
	"Description" varchar (100) not null,

	"Company" bigint references "Infrastructure"."Companies",
	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

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
	constraint "FK_PrototypeVersionTags_Version" foreign key ("Prototype", "Version") references "CheckLists"."PrototypeVersions",

	"Code" varchar (50),
	constraint "PK_PrototypeVersionTags" primary key ("Prototype", "Version", "Code")
);

create table "CheckLists"."InstanceNumerators"
(
	"Prototype" bigint not null,
	"Version" int not null,
	constraint "FK_Instances_Version" foreign key ("Prototype", "Version") references "CheckLists"."PrototypeVersions" on update cascade,

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
$body$ 
begin
	set _Number = 1;

	select 
		_Number = isnull(n."Number", 0) + 1
	from 
		"CheckLists"."InstanceNumerators" n 
	where
		n."Prototype" = _Prototype
		and n."Version" = _Version
		and n."Year" = _Year;

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
$body$ language plpgsql;

create table "CheckLists"."InstanceStates"
(
	"Code" varchar (20) primary key
);

insert into "CheckLists"."InstanceStates" 
values 
	('In progress'),
	('Confirmed'),
	('Canceled'),
	('Simulation evidence');

create table "CheckLists"."Instances"
(
    "ID" bigserial primary key,
    
	"Prototype" bigint references "CheckLists"."Prototypes",
	"Version" int,
	"Year" int,
	constraint "FK_Instances_InstanceNumerator" foreign key ("Prototype", "Version", "Year") references "CheckLists"."InstanceNumerators",

	"PrototypeCode" varchar (50),
	"Number" int,

	"InstanceState" varchar (20) not null references "CheckLists"."InstanceStates",

	"Description" text,
	"Data" text, --replace with no sql table

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_Instance_Number" on "CheckLists"."Instances" ("Prototype", "Version", "Year", "Number");
create unique index "Index_Instance_Code" on "CheckLists"."Instances" ("PrototypeCode", "Version", "Year", "Number");

create table "CheckLists"."InstanceTags"
(
	"ID" bigserial primary key,
	"Instance" bigint not null references "CheckLists"."Instances",
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