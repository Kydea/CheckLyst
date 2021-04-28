alter table "Trail"."Records" add column "CheckListID" bigint;
alter table "Trail"."Records" rename column "CheckList" to "CheckListCode";
insert into "Trail"."Actions" values ('Instance external source row imported');
alter table "Security"."Users" add column "CorporateID" varchar(50);

create table "CheckLists"."InstanceRelations"
(
	"Parent" bigint references "CheckLists"."Instances" on delete cascade,
	"Item" int,
	constraint "PK_InstanceRelations" primary key ("Parent", "Item"),

	"Child" bigint references "CheckLists"."Instances" on delete cascade,
	constraint "InstanceRelations_SelfReference" check ("Parent" <> "Child")
);

create index "Index_InstanceRelations_Child" on "CheckLists"."InstanceRelations"("Child");

alter table "CheckLists"."InstanceExpressionValues" alter column "NotificationNote" type varchar (200);
alter table "CheckLists"."InstanceExpressionValues" alter column "NotificationEmail" type varchar (200);
