alter table "CheckLists"."InstanceExpressionValues" alter "Unique" set default null;
alter table "CheckLists"."InstanceExpressionValues" alter "Unique" type int using case when "Unique" then 1 else 0 end;

--Script3.CheckLists.sql > create or replace function "CheckLists"."ValidateUniqueExpressions"...
