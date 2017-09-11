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
    "Company" bigint,
	"User" varchar (60),
    constraint "PasswordHistory_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",
	
	"FunctionGroup" varchar (20),
	"Function" varchar (20),

	constraint "PK_UserFunctions" primary key ("Company", "User", "PrototypeType", "FunctionGroup", "Function"),

	"Tag" varchar (50),

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

--insert into Security.Users (Code, Description, Password, Enabled, SettingsManager) values ('CheckListManager', 'System administrator', 0x5b852905cefc6d59524c45b8f642927b5459c2f622d40002466109e69dbccc36, 1, 1); --V4sc0 R0ss1, sha256
