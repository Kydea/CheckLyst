alter table "CheckLists"."PrototypeTypes" add "LifeCycle" int;
alter table "CheckLists"."PrototypeTypes" add constraint "PrototypeTypes_LifeCycle" check ("LifeCycle" > 0);
alter table "CheckLists"."PrototypeVersions" add "LifeCycle" int;
alter table "CheckLists"."PrototypeVersions" add constraint "Prototypes_LifeCycle" check ("LifeCycle" > 0);
