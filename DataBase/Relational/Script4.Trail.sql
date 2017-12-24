create schema "Trail";

create table "Trail"."Actions"
(
    "Code" varchar (100) primary key
);

insert into "Trail"."Actions"
values 
	('Email failed'),

	('Login'),
	('Logout'),
	('Login failed'),

	('User created'),
	('User edited'),
	('User deleted'),

	('Prototype created'),
	('Prototype deleted'),
	('Prototype state changed'),
	('Prototype state change proposed'),
	('Prototype signature issued');

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
