create schema "Trail";

create table "Trail"."Actions"
(
    "Code" varchar (100) primary key
);

--fino qui

create table "Trail"."Records"
(
	"ID" bigserial primary key,

    "Company" bigint,
	"User" varchar (60),
    constraint "FK_Records_User" foreign key ("Company", "User") references "Security"."Users",

    "Date" timestamptz not null default current_timestamp,

    "Action" varchar (100) references "Trail"."Actions",
	"Information" text,

	"Prototype" bigint,
	"Version" int,

	"Instance" bigint,

	check("Prototype" is null and "Version" is null or "Instance" is null)
);

create index "Index_Records_PrototypeVersion" on "Trail"."Records" ("Company", "Prototype", "Version");
create index "Index_Records_Instance" on "Trail"."Records" ("Company", "Instance");
create index "Index_Records_User" on "Trail"."Records" ("Company", "User");