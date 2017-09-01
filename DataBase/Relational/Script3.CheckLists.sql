create schema "CheckLists";

create table "CheckLists"."PrototypeTypes"
(
	"ID" bigserial,
	"Description" varchar (100),

	"Company" bigint references "Infrastructure"."Companies",
	"Department" bigint references "Infrastructure"."Departments",

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz,
);

create table CheckLists.Prototypes
(
	"ID" bigserial,
	"Name" varchar (50) primary key,
	"Description" varchar (100) not null,

	"Company" bigint references "Infrastructure"."Companies",
	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz,
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

	"Data" text, --replace with no sql table

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz,

);

create trigger CheckLists.Versions_Validate on CheckLists.Versions for insert, update as
begin

	declare @Count int = -1;
	declare @Prototype varchar(50) = 'not set';

	select
		@Count = count(*), @Prototype = v.Prototype
	from
		CheckLists.Versions v
	where
		v.Prototype in (select i.Prototype from Inserted i)
		and v.VersionState = 'Draft'
	group by v.Prototype
	having count(*) > 1

	if @Count > 1
	begin
		raiserror ('A draft version of the prototype %s already exists', 16, 30, @Prototype);
		rollback tran;
		return;
	end

end

create trigger CheckLists.Versions_Update on CheckLists.Versions for update as

insert into Logs.Events(Action, [User], Date, UTCDate, Information, Checklist)
select
	case when isnull(i.ModifiedBy, '(System)') = '(System)' then 'Prototype state auto-changed' else 'Prototype state changed' end,
	isnull(i.ModifiedBy, '(System)'),
	i.ModifiedOn,
	getutcdate(),
	p.Code + '.' + p.Description + '.' + cast(v.Version as varchar(10)) + ' - State: Obsolete',
	p.Code + '.' + cast(v.Version as varchar(10))
from
	inserted i
	inner join deleted d on d.Prototype = i.Prototype and d.Version = i.Version
	inner join CheckLists.Versions v on i.Prototype = v.Prototype and v.Version <> i.Version
	inner join CheckLists.Prototypes p on v.Prototype = p.Code
where
	i.VersionState = 'Active'
	and i.VersionState <> d.VersionState
	and v.VersionState = 'Active'

insert into CheckLists.History (Prototype, Version, Date, UTCDate, State, [User]) 
select v.Prototype, v.Version, v.ModifiedOn, getutcdate(), 'Obsolete', isnull(i.ModifiedBy, '(System)')
from 
	inserted i
	inner join deleted d on d.Prototype = i.Prototype and d.Version = i.Version
	inner join CheckLists.Versions v on i.Prototype = v.Prototype and v.Version <> i.Version
where
	i.VersionState = 'Active'
	and i.VersionState <> d.VersionState
	and v.VersionState = 'Active';

update v set 
	VersionState = 'Obsolete', 
	v.ValidUntil = isnull(v.ValidUntil, getutcdate()),
	ModifiedOn = i.ModifiedOn,
	UTCModifiedOn = getutcdate()
from
	inserted i
	inner join deleted d on d.Prototype = i.Prototype and d.Version = i.Version
	inner join CheckLists.Versions v on i.Prototype = v.Prototype and v.Version <> i.Version
where
	i.VersionState = 'Active'
	and i.VersionState <> d.VersionState
	and v.VersionState = 'Active';

create procedure CheckLists.UpdateStates (@Company varchar (50), @LocalDate datetime)
as
	-- Approved > Active

	insert into Logs.Events(Action, [User], Date, UTCDate, Information, Checklist) 
	select 'Prototype state auto-changed', '(System)', @LocalDate, getutcdate(),
		p.Code + '.' + isnull(v.Description, p.Description) + '.' + cast(v.Version as varchar(10)) + ' - State: Active',
		p.Code + '.' + cast(v.Version as varchar(10))
	from 
		CheckLists.Versions v
		inner join CheckLists.Prototypes p on v.Prototype = p.Code 
	where
		p.Company = @Company
		and v.VersionState = 'Approved'
		and v.ValidFrom <= getutcdate();

	insert into CheckLists.History (Prototype, Version, Date, UTCDate, State, [User]) 
	select v.Prototype, v.Version, @LocalDate, getutcdate(), 'Active', '(System)'
	from 
		CheckLists.Versions v
		inner join CheckLists.Prototypes p on v.Prototype = p.Code 
	where
		p.Company = @Company
		and v.VersionState = 'Approved'
		and v.ValidFrom <= getutcdate();

	update v 
	set 
		VersionState = 'Active',
		ModifiedOn = @LocalDate,
		UTCModifiedOn = getutcdate()
	from 
		CheckLists.Versions v
		inner join CheckLists.Prototypes p on v.Prototype = p.Code 
	where
		p.Company = @Company
		and v.VersionState = 'Approved'
		and v.ValidFrom <= getutcdate();

	-- Active > Obsolete

	insert into Logs.Events(Action, [User], Date, UTCDate, Information, Checklist) 
	select 'Prototype state auto-changed', '(System)', @LocalDate, getutcdate(),
		p.Code + '.' + p.Description + '.' + cast(v.Version as varchar(10)) + ' - State: Obsolete',
		p.Code + '.' + cast(v.Version as varchar(10))
	from 
		CheckLists.Versions v
		inner join CheckLists.Prototypes p on v.Prototype = p.Code 
	where
		p.Company = @Company
		and v.VersionState = 'Active'
		and v.ValidUntil is not null and v.ValidUntil <= getutcdate();

	insert into CheckLists.History (Prototype, Version, Date, UTCDate, State, [User]) 
	select v.Prototype, v.Version, @LocalDate, getutcdate(), 'Obsolete', '(System)'
	from 
		CheckLists.Versions v
		inner join CheckLists.Prototypes p on v.Prototype = p.Code 
	where
		p.Company = @Company
		and v.VersionState = 'Active'
		and v.ValidUntil is not null and v.ValidUntil <= getutcdate();

	update v
	set 
		VersionState = 'Obsolete',
		ModifiedOn = @LocalDate,
		UTCModifiedOn = getutcdate()
	from 
		CheckLists.Versions v
		inner join CheckLists.Prototypes p on v.Prototype = p.Code 
	where
		p.Company = @Company
		and v.VersionState = 'Active'
		and v.ValidUntil is not null and v.ValidUntil <= getutcdate();

create table "CheckLists"."VersionTags"
(
	"Prototype" bigint,
	"Version" int,
	constraint "FK_VersionTags_Version" foreign key ("Prototype", "Version") references "CheckLists"."PrototypeVersions",

	"Code" varchar (50),
	constraint "PK_VersionTags" primary key ("Prototype", "Version", "Code"),
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

go

create procedure CheckLists.GetInstanceNumber 
(
	@Prototype varchar (50),
	@Version int,
	@Year int,
	@Number int out 
) as

	set @Number = 1;

	select 
		@Number = isnull(n.Number, 0) + 1
	from 
		CheckLists.InstanceNumerators n 
	where
		n.Prototype = @Prototype
		and n.Version = @Version
		and n.Year = @Year;

	if @Number = 1
		insert into CheckLists.InstanceNumerators(Prototype, Version, Year, Number)
		values (@Prototype, @Version, @Year, @Number);
	else
		update CheckLists.InstanceNumerators
		set Number = @Number
		where
			Prototype = @Prototype
			and Version = @Version
			and Year = @Year;

create table "CheckLists"."InstanceStates"
(
	"Code" varchar (20) primary key
);

insert into CheckLists.InstanceStates 
values 
	('In progress'),
	('Confirmed'),
	('Canceled'),
	('Simulation evidence');

create table "CheckLists"."Instances"
(
    "ID" bigserial,
    
	"Prototype" bigint,
	"Version" int,
	"Year" int,
	constraint "FK_Instances_InstanceNumerator" foreign key ("Prototype", "Version", "Year") references "CheckLists"."InstanceNumerators",

	"Number" int,

	"InstanceState" varchar (20) not null references "CheckLists"."InstanceStates",

	"Description" text,
	"Data" text, --replace with no sql table

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz,
);

create unique index "Index_InstanceExpressionValues" on "CheckLists"."Instances" ("Prototype", "Version", "Year", "Number");

create table CheckLists.InstanceTags
(
	"ID" bigserial primary key,
	
	"Instance" bigint not null references "Checklists"."Instances",

	"Tag" varchar (50) not null,
	constraint "FK_InstanceTags_VersionTag" foreign key ("Prototype", "Version", "Tag") references "CheckLists"."VersionTags",

	"Value" text not null
);

create index "Index_InstanceTags_Tag" on "CheckLists"."InstanceTags"("Insance", "Tag");

create table "CheckLists"."InstanceExpressionValues"
(
	"Instance" bigint not null references "Checklists"."Instances",
	"Code" varchar (50),

	constraint "PK_InstanceExpressionValues" primary key ("Instance", "Code"),

	"Value" text not null
);