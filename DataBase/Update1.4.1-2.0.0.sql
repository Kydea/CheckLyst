alter table "CheckLists"."InstanceExpressionValues" add 
	"Description" varchar (100),
	"Unique" boolean not null default true,
	"Exception" boolean not null default false;

alter table "WorkFlows"."WorkFlowApplicationSteps" add "UserDescription" varchar (100);

update "WorkFlows"."WorkFlowApplicationSteps"
set 
	"UserDescription" = (select u."Description" from "Security"."Users" u where u."Company" = "WorkFlowApplicationSteps"."Company" and u."Code" = "WorkFlowApplicationSteps"."User")
where 
	not "User" is null;

create or replace function "CheckLists"."ValidateUniqueExpressions"
(
	_Instance bigint
) 
returns int as
$$ 
declare
	_Year varchar(4);
	_Number int;
	_Code varchar(50);
	_Value text;
begin
	_Year = null;
	
	select 
		case when i2."Year" > 0 then cast(i2."Year" as varchar(4)) when pv."VersionState" = 'Draft' then 'TEST' else 'SE' end,
		i2."Number",
		case when iev1."Description" is null then iev1."Code" else iev1."Description" || ' (' || iev1."Code" || ')' end, iev1."Value"
	into _Year, _Number, _Code, _Value
	from
		"CheckLists"."Instances" i1
		inner join "CheckLists"."PrototypeVersions" pv on i1."Prototype" = pv."Prototype" and i1."Version" = pv."Version"
		inner join "CheckLists"."Instances" i2 on i1."Prototype" = i2."Prototype" and i1."Version" = i2."Version" and i2."InstanceState" = 'In progress' and i1."ID" <> i2."ID"
		inner join "CheckLists"."InstanceExpressionValues" iev1 on i1."ID" = iev1."Instance" and iev1."Unique"
		inner join "CheckLists"."InstanceExpressionValues" iev2 on i2."ID" = iev2."Instance" and iev1."Code" = iev2."Code" and iev1."Value" = iev2."Value" and iev1."Unique"
	where
		i1."ID" = _Instance
	limit 1;
	
	if not _Year is null then
		raise 'The value % for the expression % has been found in the in progress instance % - %', _Value, _Code, _Year, _Number;
	end if;
																	   
	return 0;
end
$$
language plpgsql;
