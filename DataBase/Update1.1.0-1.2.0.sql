delete from "DataSources"."DataSources";

alter table "DataSources"."DataSources" add column "Class" varchar (50) not null;
alter table "DataSources"."DataSources" add column "Password" varchar (50);