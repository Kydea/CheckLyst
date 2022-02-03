create function "CheckLists"."UpdateInstanceData"
(
	"_Instance" bigint,
	"_Company" bigint,
	"_ItemsByCode" text,
	"_ItemsByLabel" text,
	"_Expressions" text
) 
returns int as
$$ 
declare 
	"_ItemsByCodeJSON" jsonb := "_ItemsByCode"::jsonb;
	"_ItemsByLabelJSON" jsonb := "_ItemsByLabel"::jsonb;
	"_ExpressionsJSON" jsonb := "_Expressions"::jsonb;
begin
	insert into "CheckLists"."InstanceData" ("Instance", "Company", "ItemsByCode", "ItemsByLabel", "Expressions")
	values ("_Instance", "_Company", "_ItemsByCodeJSON", "_ItemsByLabelJSON", "_ExpressionsJSON")
	on conflict ("Instance") do update set
		"ItemsByCode" = "_ItemsByCodeJSON", 
		"ItemsByLabel" = "_ItemsByLabelJSON", 
		"Expressions" = "_ExpressionsJSON";

	return 0;
end
$$
language plpgsql;