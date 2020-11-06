--Instances ID temporary table

create table "CheckLists"."TemporaryList" ("ID" bigint primary key);

insert into "CheckLists"."TemporaryList"
select "ID"
from 
	"CheckLists"."Instances" i
where
	i."ID" in 
		(428, 4096, 5034, 5170, 5171)

--Initial state (export0.csv)

select 
	i."ID", c."Name", 
	i."PrototypeName" || '.' || i."Version" || '.' || trim(to_char(i."Year", '9999')) || '.' || trim(to_char(i."Number", '9999999999')) "InstanceCode",
	i."InstanceState", i."CreatedBy", i."CreatedOn", i."ModifiedBy", i."ModifiedOn", pt."Name", d."Name",
	ih."State", ih."User", ih."Note"	
from 
	"CheckLists"."TemporaryList" l
	inner join 	"CheckLists"."Instances" i on l."ID" = i."ID"
	left join "Infrastructure"."Companies" c on i."Company" = c."ID"
	left join "CheckLists"."Prototypes" p on i."Prototype" = p."ID"
	left join "Infrastructure"."Departments" d on p."Department" = d."ID"
	left join "CheckLists"."PrototypeTypes" pt on p."PrototypeType" = pt."ID"
	left join "CheckLists"."InstancesHistory" ih on ih."ID" in
	(
		select max(ih2."ID")
		from "CheckLists"."InstancesHistory" ih2
		where ih2."Instance" = i."ID"
	)

--Instances update

update "CheckLists"."Instances"
set	"InstanceState" = 'Confirmed'
where "ID" in (select l."ID" from "CheckLists"."TemporaryList" l);

--History update

insert into "CheckLists"."InstancesHistory" ("Instance", "State", "User", "Note")
select l."ID", 'Confirmed', 'CheckLyst (Automatic Transaction)', 'This instance has been closed according to CAPA TW 1691560 (IT Global change CHG000010651761)'
from "CheckLists"."TemporaryList" l;

--Final state (export1.csv)

select 
	i."ID", c."Name", 
	i."PrototypeName" || '.' || i."Version" || '.' || trim(to_char(i."Year", '9999')) || '.' || trim(to_char(i."Number", '9999999999')) "InstanceCode",
	i."InstanceState", i."CreatedBy", i."CreatedOn", i."ModifiedBy", i."ModifiedOn", pt."Name", d."Name",
	ih."State", ih."User", ih."Note"	
from 
	"CheckLists"."TemporaryList" l
	inner join 	"CheckLists"."Instances" i on l."ID" = i."ID"
	left join "Infrastructure"."Companies" c on i."Company" = c."ID"
	left join "CheckLists"."Prototypes" p on i."Prototype" = p."ID"
	left join "Infrastructure"."Departments" d on p."Department" = d."ID"
	left join "CheckLists"."PrototypeTypes" pt on p."PrototypeType" = pt."ID"
	left join "CheckLists"."InstancesHistory" ih on ih."ID" in
	(
		select max(ih2."ID")
		from "CheckLists"."InstancesHistory" ih2
		where ih2."Instance" = i."ID"
	)

--Temporary table deletion

drop table "CheckLists"."TemporaryList";