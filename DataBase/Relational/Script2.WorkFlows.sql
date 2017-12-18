create schema "WorkFlows";

create table "WorkFlows"."WorkFlows"
(
	"ID" bigserial primary key,

	"Company" bigint not null references "Infrastructure"."Companies",
	"Description" varchar (250) not null,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create table "WorkFlows"."WorkFlowSteps"
(
	--to sign a step, each (strictly) previous step must be signed
	--a step can be signed by any user with the corresponding function grant and the tag in WorkFlowStepTags  
	"ID" bigserial primary key,

	"WorkFlow" bigint references "WorkFlows"."WorkFlows" on delete cascade,
	"Step" int, 

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create table "WorkFlows"."WorkFlowStepTags"
(
	"WorkFlowStep" bigint references "WorkFlows"."WorkFlowSteps" on delete cascade,
	"Tag" varchar(50) not null,

	constraint "PK_WorkFlowStepTags" primary key ("WorkFlowStep", "Tag")
);

create table "WorkFlows"."WorkFlowActions"
(
	"Code" varchar(50) primary key
);

insert into "WorkFlows"."WorkFlowActions" values ('Prototype – state to Approved/Active');
insert into "WorkFlows"."WorkFlowActions" values ('Prototype – state to Obsolete');
insert into "WorkFlows"."WorkFlowActions" values ('Instance – state to Confirmed');

create table "WorkFlows"."WorkFlowApplications"
(
	"ID" bigserial primary key,

	"Prototype" bigint,
	"Version" int,
	"Instance" bigint,
	check("Prototype" is null and "Version" is null or "Instance" is null),

	"WorkFlowAction" varchar (50) references "WorkFlows"."WorkFlowActions",

	"WorkFlow" bigint references "WorkFlows"."WorkFlows",

	"Date" timestamptz,
	"Approved" boolean,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create index "Index_WorkFlowApplications_PrototypeVersion" on "WorkFlows"."WorkFlowApplications"("Prototype", "Version");
create index "Index_WorkFlowApplications_Instance" on "WorkFlows"."WorkFlowApplications"("Instance");

create unique index "Index_WorkFlowApplications_PrototypeVersion_InProgress" 
	on "WorkFlows"."WorkFlowApplications"("Prototype", "Version", "WorkFlowAction")
	where not "Prototype" is null and ("Approved" is null or "Approved");

create unique index "Index_WorkFlowApplications_Instance_InProgress" 
	on "WorkFlows"."WorkFlowApplications"("Instance", "WorkFlowAction")
	where not "Instance" is null and ("Approved" is null or "Approved");

create table "WorkFlows"."WorkFlowApplicationSteps"
(
	"ID" bigserial primary key,

	"WorkFlowApplication" bigint references "WorkFlows"."WorkFlowApplications",
	"WorkFlowStep" bigint references "WorkFlows"."WorkFlowSteps",

	"Company" bigint,
	"User" varchar (60),

    constraint "WorkFlowApplicationSteps_User" foreign key ("Company", "User") references "Security"."Users",

	"Comment" varchar (300),

	"Date" timestamptz not null default current_timestamp,
	"Approved" boolean not null
);

create unique index "Index_WorkFlowApplicationSteps_WorkFlowStep" on "WorkFlows"."WorkFlowApplicationSteps"("WorkFlowStep")
