create table "Security"."FunctionGroups"
(
	"Code" varchar (20) primary key
);

insert into "Security"."FunctionGroups" values
	('DataSources'),
	('PrototypeTypes'),
	('Departments'),
	('Instances'),
	('Prototypes'),
	('Trail'),
	('Users'),
	('WorkFlows');

create table "Security"."Functions"
(
	"FunctionGroup" varchar (20) references "Security"."FunctionGroups",
	"Code" varchar (20),

	constraint "PK_Functions" primary key ("FunctionGroup", "Code")
);

insert into "Security"."Functions" values
	('DataSources', 'Edit'),
	('DataSources', 'View'),

	('Departments', 'Edit'),
	('Departments', 'View'),

	('PrototypeTypes', 'Edit'),
	('PrototypeTypes', 'View'),

	('Instances', 'Confirm'),
	('Instances', 'Create'),
	('Instances', 'Delete'),
	('Instances', 'Edit'),
	('Instances', 'Unsing'),
	('Instances', 'View'),

	('Prototypes', 'Approve'),
	('Prototypes', 'Edit'),
	('Prototypes', 'View'),

	('Trail', 'View'),

	('Users', 'Edit'),
	('Users', 'View'),

	('WorkFlows', 'Edit'),
	('WorkFlows', 'View');

create table "Security"."UserFunctions"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"User" varchar (60) not null,
    constraint "UserFunctions_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"FunctionGroup" varchar (20) not null,
	"Function" varchar (20) not null,
	constraint "UserFunctions_Function" foreign key ("FunctionGroup", "Function") references "Security"."Functions" on update cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",
	"Tag" varchar (50),

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_UserFunctions_Function" on "Security"."UserFunctions" ("Company", "User", "FunctionGroup", "Function", "Department", "PrototypeType", "Tag");