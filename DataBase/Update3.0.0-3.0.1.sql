create or replace function "CheckLists"."UpdateInstanceData"
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

with keys as
(
	select 
		coalesce((select * from jsonb_object_keys("ItemsByCode") limit 1), '') "Key of first item by code", 
		coalesce((select * from jsonb_object_keys("ItemsByLabel") limit 1), '')  "Key of first item by label", 
		*
	from "CheckLists"."InstanceData"
), items as
(
	select 
		"ItemsByCode"->"Key of first item by code" "First item by code",
		"ItemsByLabel"->"Key of first item by label" "First item by label",
		*		
	from keys
), values as
(
	select 
		coalesce("First item by code"->>'Code', '') "Code of first item by code",
		coalesce("First item by code"->>'DescriptionValue', '') "Label of first item by code",
		coalesce("First item by label"->>'Code', '') "Code of first item by label",
		coalesce("First item by label"->>'DescriptionValue', '') "Label of first item by label",
		"First item by code" is null "No item by code",
		"First item by label" is null "No item by label",
		*		
	from items
), checks as
(
	select 
		"Key of first item by code" <> "Code of first item by code" "Code mismatch",
		"Key of first item by label" <> "Label of first item by label" "Label mismatch",
		exists (select k from (select jsonb_object_keys("ItemsByCode") k, "Instance" from "CheckLists"."InstanceData") t where t."Instance" = values."Instance" and t.k like '% %') "Code with space",
		exists (select k from (select jsonb_object_keys("ItemsByLabel") k, "Instance" from "CheckLists"."InstanceData") t where t."Instance" = values."Instance" and t.k like '% %') "Label with space",
		* 
	from values
), final as
(
	select 
		"Code mismatch" and not "Label mismatch" and not "No item by label" and "Code of first item by label" <> "Label of first item by label" 
		or not "Code mismatch" and not "No item by code" and "Code of first item by code" <> "Label of first item by code" and "Label mismatch" 
		or not "Code mismatch" and not "Label mismatch" and "Code with space" 
			"Anomaly",
		*
	from checks
)
update "CheckLists"."InstanceData"
set 
	"ItemsByCode" = "ItemsByLabel",
	"ItemsByLabel" = "ItemsByCode"
where
	"Instance" in 
	(
		select "Instance"
		from final
		where "Code mismatch" or "Label mismatch" or "Code with space"
	)
