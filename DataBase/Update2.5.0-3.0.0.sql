alter table "Trail"."Records" add column "CheckListID" bigint;
alter table "Trail"."Records" rename column "CheckList" to "CheckListCode";
insert into "Trail"."Actions" values ('Instance external source row imported');
