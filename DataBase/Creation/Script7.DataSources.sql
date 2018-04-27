create schema "DataSources";

create table "DataSources"."DataSources"
(
	"ID" bigserial primary key,

	"Company" bigint not null,	
	"Code" varchar (30) not null,

	"Description" varchar (100),

	"Class" varchar (50) not null,
	"Password" varchar (50),	
	"Data" text, 

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);