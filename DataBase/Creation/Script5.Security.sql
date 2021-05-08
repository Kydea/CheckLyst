create table "Security"."FunctionGroups"
(
	"Code" varchar (20) primary key
);

insert into "Security"."FunctionGroups" values
	('Companies'),
	('DataSources'),
	('Departments'),
	('FTPServers'),
	('Instances'),
	('Prototypes'),
	('PrototypeTypes'),
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
	('Instances', 'Unsign'),
	('Instances', 'View'),

	('Prototypes', 'Approve'),
	('Prototypes', 'Edit'),
	('Prototypes', 'Preview'),
	('Prototypes', 'View'),

	('Trail', 'View'),

	('Users', 'Edit'),
	('Users', 'View'),

	('WorkFlows', 'Edit'),
	('WorkFlows', 'View'),

	('FTPServers', 'Edit'),
	('FTPServers', 'View'),

	('Companies', 'Edit'),
	('Companies', 'View');

create table "Security"."UserLevels"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"User" varchar (60) not null,
    constraint "UserFunctions_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"SignatureLevel" varchar (50),
	"PrototypeWorkFlowLevel" varchar (50),
	"InstanceWorkFlowLevel" varchar (50),

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_UserLevels_Level" on "Security"."UserLevels" ("Company", "User", "Department", "PrototypeType");

create table "Security"."Roles"
(
    "Company" bigint references "Infrastructure"."Companies",
	"Code" varchar (60),

    constraint "Roles_PK" primary key ("Company", "Code"),
    
	"FunctionGroup" varchar (20) references "Security"."FunctionGroups", --Prototypes, Instances o null
    
	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Roles_Code" on "Security"."Roles" ("Company", upper("Code"));

create table "Security"."RoleFunctions"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"Role" varchar (60) not null,
    constraint "RoleFunctions_Role" foreign key ("Company", "Role") references "Security"."Roles" on update cascade on delete cascade,

	"FunctionGroup" varchar (20) not null,
	"Function" varchar (20) not null,
	constraint "RoleFunctions_Function" foreign key ("FunctionGroup", "Function") references "Security"."Functions" on update cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_RoleFunctions_Function" on "Security"."RoleFunctions" ("Company", "Role", "FunctionGroup", "Function");

create table "Security"."UserRoles"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"User" varchar (60) not null,
    constraint "UserRoles_Group" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"Role" varchar (60) not null,
    constraint "UserRoles_Role" foreign key ("Company", "Role") references "Security"."Roles" on update cascade on delete cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_UserRoles_Role" on "Security"."UserRoles" ("Company", "User", "Department", "PrototypeType");

create table "Security"."UserFunctions"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"User" varchar (60) not null,
    constraint "UserFunctions_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"FunctionGroup" varchar (20) not null,
	"Function" varchar (20) not null,
	constraint "UserFunctions_Function" foreign key ("FunctionGroup", "Function") references "Security"."Functions" on update cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_UserFunctions_Function" on "Security"."UserFunctions" ("Company", "User", "Department", "PrototypeType", "FunctionGroup", "Function");

create table "Security"."Groups"
(
    "Company" bigint references "Infrastructure"."Companies",
	"Code" varchar (60),
    
    constraint "Groups_PK" primary key ("Company", "Code"),
    
	"Description" varchar (100) not null,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Groups_Code" on "Security"."Groups" ("Company", upper("Code"));

create table "Security"."GroupRoles"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"Group" varchar (60) not null,
    constraint "GroupRoles_Group" foreign key ("Company", "Group") references "Security"."Groups" on update cascade on delete cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"Role" varchar (60) not null,
	constraint "GroupRoles_Role" foreign key ("Company", "Role") references "Security"."Roles" on update cascade on delete cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_GroupRoles_Role" on "Security"."GroupRoles" ("Company", "Group", "Department", "PrototypeType");

create table "Security"."GroupFunctions"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"Group" varchar (60) not null,
    constraint "GroupFunctions_Group" foreign key ("Company", "Group") references "Security"."Groups" on update cascade on delete cascade,

	"FunctionGroup" varchar (20) not null,
	"Function" varchar (20) not null,
	constraint "GroupFunctions_Function" foreign key ("FunctionGroup", "Function") references "Security"."Functions" on update cascade,

	"Department" bigint references "Infrastructure"."Departments",
	"PrototypeType" bigint references "CheckLists"."PrototypeTypes",

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_GroupFunctions_Function" on "Security"."GroupFunctions" ("Company", "Group", "Department", "PrototypeType", "FunctionGroup", "Function");

create table "Security"."UserGroups"
(
	"ID" bigserial primary key,

    "Company" bigint not null,
	"User" varchar (60) not null,
    constraint "UserRoles_Group" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,

	"Group" varchar (60) not null,
    constraint "UserGroups_Group" foreign key ("Company", "Group") references "Security"."Groups" on update cascade on delete cascade,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

create unique index "Index_UserGroups_Group" on "Security"."UserGroups" ("Company", "Group", "User");

create function "Security"."GetUserGrants"(_Company bigint, _Code varchar(60), _FunctionGroup varchar(20), _Function varchar(20))
returns table ("Department" bigint, "PrototypeType" bigint) as
$func$
																															 
	select uf."Department", uf."PrototypeType"
	from "Security"."UserFunctions" uf
	where
		uf."Company" = _Company
		and uf."User" = _Code
		and uf."FunctionGroup" = _FunctionGroup
		and uf."Function" = _Function
		and not uf."Department" is null
	union
	select gf."Department", gf."PrototypeType"
	from 
		"Security"."UserGroups" ug
		inner join "Security"."GroupFunctions" gf on ug."Company" = gf."Company" and ug."Group" = gf."Group"
	where
		ug."Company" = _Company
		and ug."User" = _Code
		and gf."FunctionGroup" = _FunctionGroup
		and gf."Function" = _Function
		and not gf."Department" is null
	union
	select ur."Department", ur."PrototypeType"
	from 
		"Security"."UserRoles" ur
		inner join "Security"."RoleFunctions" rf on ur."Company" = rf."Company" and ur."Role" = rf."Role"
	where
		ur."Company" = _Company
		and ur."User" = _Code
		and rf."FunctionGroup" = _FunctionGroup
		and rf."Function" = _Function
		and not ur."Department" is null
	union
	select gr."Department", gr."PrototypeType"
	from 
		"Security"."UserGroups" ug
		inner join "Security"."GroupRoles" gr on ug."Company" = gr."Company" and ug."Group" = gr."Group"
		inner join "Security"."RoleFunctions" rf on gr."Company" = rf."Company" and gr."Role" = rf."Role"
	where
		ug."Company" = _Company
		and ug."User" = _Code
		and rf."FunctionGroup" = _FunctionGroup
		and rf."Function" = _Function
		and not gr."Department" is null

$func$ language sql;

create function "Security"."GetAllUserGrants"(_Company bigint, _Code varchar(60))
returns table ("Department" bigint, "PrototypeType" bigint, "Role" varchar(60), "FunctionGroup" varchar(20), "Function" varchar(20)) as
$func$
																															 
	select uf."Department", uf."PrototypeType", null, uf."FunctionGroup", uf."Function"
	from "Security"."UserFunctions" uf
	where
		uf."Company" = _Company
		and uf."User" = _Code
	union
	select gf."Department", gf."PrototypeType", null, gf."FunctionGroup", gf."Function"
	from 
		"Security"."UserGroups" ug
		inner join "Security"."GroupFunctions" gf on ug."Company" = gf."Company" and ug."Group" = gf."Group"
	where
		ug."Company" = _Company
		and ug."User" = _Code
	union
	select ur."Department", ur."PrototypeType", ur."Role", null, null
	from 
		"Security"."UserRoles" ur
	where
		ur."Company" = _Company
		and ur."User" = _Code
	union
	select gr."Department", gr."PrototypeType", gr."Role", null, null
	from 
		"Security"."UserGroups" ug
		inner join "Security"."GroupRoles" gr on ug."Company" = gr."Company" and ug."Group" = gr."Group"
	where
		ug."Company" = _Company
		and ug."User" = _Code

$func$ language sql;

create function "Security"."GetGrantedUsers"(_Company bigint, _Department bigint, _PrototypeType bigint, _FunctionGroup varchar(20), _Function varchar(20))
returns setof "Security"."Users" as
$func$
																															 
	select u.*
	from "Security"."Users" u
	where
		u."Company" = _Company
		and (
			exists (
				select 1 
				from "Security"."UserFunctions" uf
				where
					uf."Company" = u."Company"
					and uf."User" = u."Code"
					and uf."FunctionGroup" = _FunctionGroup
					and uf."Function" = _Function
					and uf."Department" = _Department
					and (uf."PrototypeType" is null or uf."PrototypeType" = _PrototypeType)
			)
			or exists (
				select 1
				from 
					"Security"."UserGroups" ug
					inner join "Security"."GroupFunctions" gf on ug."Company" = gf."Company" and ug."Group" = gf."Group"
				where
					ug."Company" = u."Company"
					and ug."User" = u."Code"
					and gf."FunctionGroup" = _FunctionGroup
					and gf."Function" = _Function
					and gf."Department" = _Department
					and (gf."PrototypeType" is null or gf."PrototypeType" = _PrototypeType)
			)
			or exists (
				select 1
				from 
					"Security"."UserRoles" ur
					inner join "Security"."RoleFunctions" rf on ur."Company" = rf."Company" and ur."Role" = rf."Role"
				where
					ur."Company" = u."Company"
					and ur."User" = u."Code"
					and rf."FunctionGroup" = _FunctionGroup
					and rf."Function" = _Function
					and ur."Department" = _Department
					and (ur."PrototypeType" is null or ur."PrototypeType" = _PrototypeType)
			)
			or exists (
				select 1
				from 
					"Security"."UserGroups" ug
					inner join "Security"."GroupRoles" gr on ug."Company" = gr."Company" and ug."Group" = gr."Group"
					inner join "Security"."RoleFunctions" rf on gr."Company" = rf."Company" and gr."Role" = rf."Role"
				where
					ug."Company" = u."Company"
					and ug."User" = u."Code"
					and rf."FunctionGroup" = _FunctionGroup
					and rf."Function" = _Function
					and gr."Department" = _Department
					and (gr."PrototypeType" is null or gr."PrototypeType" = _PrototypeType)
			)
		)
$func$ language sql;