create unique index "Departments_Name" on "Infrastructure"."Departments" ("Company", upper("Name"));
create unique index "PrototypeTypes_Name" on "CheckLists"."PrototypeTypes" ("Company", "Department", upper("Name"));
create unique index "WorkFlows_Description" on "WorkFlows"."WorkFlows" ("Company", upper("Description"));

drop index "Index_PrototypeVersions_Draft" on "CheckLists"."PrototypeVersions";
create unique index "Index_PrototypeVersions_Active" on "CheckLists"."PrototypeVersions"("Prototype") where "VersionState" in ('Active');
create unique index "Index_PrototypeVersions_NewVersion" on "CheckLists"."PrototypeVersions"("Prototype") where "VersionState" in ('Draft', 'In simulation', 'In approval');

delete from "Configuration"."SettingValues" where "Setting" = 'FtpConnection';
delete from "Configuration"."Settings" where "Code" = 'FtpConnection';

insert into "Configuration"."Settings" ("Code") values
	('InstancesFtpConnection'),
	('PrototypesFtpConnection');
	
create index "Index_InstanceExpressionValues_Code" on "CheckLists"."InstanceExpressionValues"("Instance", "Code");
create unique index "Index_DataSources_Company_Code" on "DataSources"."DataSources" ("Company", "Code");
	
create or replace function "CheckLists"."GetInstanceNumber"
(
	_Prototype bigint,
	_Version int,
	_Year int
) 
returns int as
$$ 
declare
	_Number int;
begin
	_Number = 1;

	lock table only "CheckLists"."InstanceNumerators" in row exclusive mode;
	
	select coalesce(n."Number", 0) + 1 into _Number
	from 
		"CheckLists"."InstanceNumerators" n 
	where
		n."Prototype" = _Prototype
		and n."Version" = _Version
		and n."Year" = _Year;

	_Number = coalesce(_Number, 1);

	if _Number = 1 then
		insert into "CheckLists"."InstanceNumerators"("Prototype", "Version", "Year", "Number")
		values (_Prototype, _Version, _Year, _Number);
	else
		update "CheckLists"."InstanceNumerators"
		set "Number" = _Number
		where
			"Prototype" = _Prototype
			and "Version" = _Version
			and "Year" = _Year;
	end if;
    
    return _Number;
end 
$$ language plpgsql;

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
		i2."Number", iev1."Code", iev1."Value"
	into _Year, _Number, _Code, _Value
	from
		"CheckLists"."Instances" i1
		inner join "CheckLists"."PrototypeVersions" pv on i1."Prototype" = pv."Prototype" and i1."Version" = pv."Version"
		inner join "CheckLists"."Instances" i2 on i1."Prototype" = i2."Prototype" and i1."Version" = i2."Version" and i2."InstanceState" = 'In progress' and i1."ID" <> i2."ID"
		inner join "CheckLists"."InstanceExpressionValues" iev1 on i1."ID" = iev1."Instance"
		inner join "CheckLists"."InstanceExpressionValues" iev2 on i2."ID" = iev2."Instance" and iev1."Code" = iev2."Code" and iev1."Value" = iev2."Value"
	where
		i1."ID" = _Instance
	limit 1;
	
	if not _Year is null then
		raise 'The value % for the expression % has been found in the in progress instance % - %', _Value, _Code, _Year, _Number;
	end if;
																	   
	return 0;
end
$$ language plpgsql;

--for each company id
insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") values
	(id, 'InstancesFtpConnection', '{ "Host": "www.ruggerogiovannelli.org", "Port": 21, "Folder": "ruggerogiovannelli.org/Data/P", "UserName": "?", "Password": "?", "UsePassive": "true", "EnableSSL": "false" }'),
	(id, 'PrototypesFtpConnection', '{ "Host": "www.ruggerogiovannelli.org", "Port": 21, "Folder": "ruggerogiovannelli.org/Data/I", "UserName": "?", "Password": "?", "UsePassive": "true", "EnableSSL": "false" }');

