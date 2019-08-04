create schema "Trail";

create table "Trail"."Actions"
(
    "Code" varchar (100) primary key
);

insert into "Trail"."Actions"
values 
	('Email failed'),
	('Email sent'),

	('Login'),
	('Logout'),
	('Login failed'),

	('User created'),
	('User edited'),
	('User deleted'),

	('Prototype created'),
	('Prototype deleted'),
	('Prototype state auto-changed'),
	('Prototype state changed'),
	('Prototype state change proposed'),
	('Prototype signature issued'),
	('Prototype export completed'),
	('Prototype export failed'),


	('Instance created'),
	('Instance opened'),
	('Instance quitted'),
	('Instance state changed'),
	('Instance signature issued'),
	('Instance signature failed'),
	('Instance signature canceled'),
	('Instance export completed'),
	('Instance export failed'),
	('Instance value changed'),
	('Instance value copied'),
	('Instance iteration added'),
	('Instance iteration deleted'),
	('Instance iteration moved up'),
	('Instance iteration moved down'),
	('Instance external source row changed'),

	('Job error'),

	('Workflow signature issued'),
	('Workflow signature failed');


create table "Trail"."Records"
(
	"ID" bigserial primary key,

    "Company" bigint,
	"User" varchar (60),
    "Date" timestamptz not null default current_timestamp,
    "Action" varchar (100) references "Trail"."Actions",
	"Information" text,

	"CheckList" varchar (100)
);

create index "Index_Records_CheckList" on "Trail"."Records" ("Company", "CheckList");
create index "Index_Records_User" on "Trail"."Records" ("Company", "User");
