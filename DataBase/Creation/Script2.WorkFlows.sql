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

create unique index "WorkFlows_Description" on "WorkFlows"."WorkFlows" ("Company", upper("Description"));

create table "WorkFlows"."WorkFlowSteps"
(
	--to sign a step, each (strictly) previous step must be signed
	--a step can be signed by any user with the corresponding function grant and the tag in WorkFlowStepTags  
	"ID" bigserial primary key,

	"WorkFlow" bigint references "WorkFlows"."WorkFlows" on delete cascade,
	"Step" int not null,
	"Reason" varchar (50) not null,

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
	check("Prototype" is null and "Version" is null or not "Prototype" is null and not "Version" is null),
	--the followin constraint is added in the script Script3.CheckLists.sql
	--constraint "WorkFlowApplications_PrototypeVersion" foreign key ("Prototype", "Version") references "CheckLists"."PrototypeVersions" on delete cascade,

	"Instance" bigint, 
	--the followin constraint is added in the script Script3.CheckLists.sql
	--constraint "WorkFlowApplications_Instance" foreign key ("Instance") references "CheckLists"."Instances" on delete cascade,

	"Document" bigint, 
	"DocumentVersion" int,
	check("Document" is null and "DocumentVersion" is null or not "Document" is null and not "DocumentVersion" is null),
	--the followin constraint is added in the script Script8.Documents.sql
	--constraint "WorkFlowApplications_DocumentVersion" foreign key ("Document", "Version") references "Documents"."DocumentVersions" on delete cascade,

	constraint Check_WorkFlowApplications_Target check
	(
		not "Prototype" is null and "Instance" is null and "Document" is null 
		or "Prototype" is null and not "Instance" is null and "Document" is null 
		or "Prototype" is null and "Instance" is null and not "Document" is null 
	),

	"WorkFlowAction" varchar (50) references "WorkFlows"."WorkFlowActions",

	"WorkFlow" bigint references "WorkFlows"."WorkFlows",

	"Date" timestamptz,
	"Approved" boolean,
	"FinalState" varchar (20),

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

	"WorkFlowApplication" bigint,
	constraint "WorkFlows_WorkFlowApplication" foreign key ("WorkFlowApplication") references "WorkFlows"."WorkFlowApplications" on delete cascade,
	"Step" int not null,
	"Reason" varchar (50) not null,

	"Company" bigint,
	"User" varchar (60),
	"UserDescription" varchar (100),

    constraint "WorkFlowApplicationSteps_User" foreign key ("Company", "User") references "Security"."Users",

	"Comment" varchar (300),

	"Date" timestamptz,
	"Approved" boolean
);

create  index "Index_WorkFlowApplicationSteps_Step" on "WorkFlows"."WorkFlowApplicationSteps"("WorkFlowApplication", "Step");

create table "WorkFlows"."WorkFlowApplicationStepTags"
(
	"WorkFlowApplicationStep" bigint references "WorkFlows"."WorkFlowApplicationSteps" on delete cascade,
	"Tag" varchar(50) not null,

	constraint "PK_WorkFlowApplicationStepTags" primary key ("WorkFlowApplicationStep", "Tag")
);