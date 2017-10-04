create table "Security"."FunctionGroups"
(
	"Code" varchar (20) primary key
);

insert into "Security"."FunctionGroups" values
	('Platform'),
	('Company'),
	('Prototype'),
	('Instances');

create table "Security"."Functions"
(
	"FunctionGroup" varchar (20),
	"Code" varchar (20),

	constraint "PK_Functions" primary key ("FunctionGroup", "Code")
);

insert into "Security"."Functions" values
	('Company', 'Profiles'),
	('Company', 'PrototypeTypes'),
	('Company', 'AuditTrail'),
	('Company', 'Data'),

	('Prototype', 'Approve'),
	('Prototype', 'Design'),
	('Prototype', 'View'),

	('Instance', 'Create'),
	('Instance', 'Edit'),
	('Instance', 'Delete'),
	('Instance', 'Approve'),
	('Instance', 'Unlock'),
	('Instance', 'View');

create table "Security"."UserFunctions"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"User" varchar (60) not null,
    constraint "UserFunctions_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"FunctionGroup" varchar (20) not null,
	"Function" varchar (20) not null,
	constraint "UserFunctions_Function" foreign key ("FunctionGroup", "Function") references "Security"."Functions" on update cascade,

	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",
	"Tag" varchar (50),

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_UserFunctions_Function" on "Security"."UserFunctions" ("Company", "User", "FunctionGroup", "Function", "PrototypeType", "Tag");
