alter table "CheckLists"."PrototypeTypes" add "LifeCycle" int;
alter table "CheckLists"."PrototypeTypes" add constraint "PrototypeTypes_LifeCycle" check ("LifeCycle" is null or "LifeCycle" > 0);
alter table "CheckLists"."PrototypeVersions" add "LifeCycle" int;
alter table "CheckLists"."PrototypeVersions" add constraint "_LifeCycle" check ("LifeCycle" is null or "LifeCycle" > 0);

insert into "Trail"."Actions"
values 
	('Prototype changed');
