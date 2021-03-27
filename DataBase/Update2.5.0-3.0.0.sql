alter table "Trail"."Records" add column "CheckListID" bigint;
alter table "Trail"."Records" rename column "CheckList" to "CheckListCode";