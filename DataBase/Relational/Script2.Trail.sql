create schema "Trail";

create table "Trail"."Actions"
(
    "Code" varhcar (100) primary key
);

create table "Trail"."Records"
(
	"ID" bigserial primary key,

    "Company" bigint references "Infrastructure".Companies",
	"User" varchar (60).
    constraint "FK_Records_User" foreign key ("Company", "User") references "Security"."Users",

    "Date" timestamptz not null default current_time,

    "Action" varchar (100) references "Trail"."Actions",
	"Information" text,
	"Prototype" bigint references "Checklists"."PrototypeVersions",
	"Instance" bigint references "Checklists"."Instances",

	check("Prototype" is null or "Instance" is null))
);

create index "Index_Records_Prototype" on "Trail"."Records" ("Company", "Checklist");
create index "Index_Records_User" on "Trail"."Records" ("Company", "User");