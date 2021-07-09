alter table "Security"."UserLevels" enable row level security;
grant select on table "Security"."UserLevels" to "ReportRole";
create policy "Company1UserLevels" on "Security"."UserLevels" to "ReportRole1" using (true);

alter table "Security"."UserFunctions" enable row level security;
grant select on table "Security"."UserFunctions" to "ReportRole";
create policy "Company1UserFunctions" on "Security"."UserFunctions" to "ReportRole1" using (true);

alter table "Security"."UserRoles" enable row level security;
grant select on table "Security"."UserRoles" to "ReportRole";
create policy "Company1UserRoles" on "Security"."UserRoles" to "ReportRole1" using (true);

alter table "Security"."UserGroups" enable row level security;
grant select on table "Security"."UserGroups" to "ReportRole";
create policy "Company1UserGroups" on "Security"."UserGroups" to "ReportRole1" using (true);

alter table "Security"."RoleFunctions" enable row level security;
grant select on table "Security"."RoleFunctions" to "ReportRole";
create policy "Company1RoleFunctions" on "Security"."RoleFunctions" to "ReportRole1" using (true);

alter table "Security"."GroupFunctions" enable row level security;
grant select on table "Security"."GroupFunctions" to "ReportRole";
create policy "Company1GroupFunctions" on "Security"."GroupFunctions" to "ReportRole1" using (true);

alter table "Security"."GroupRoles" enable row level security;
grant select on table "Security"."GroupRoles" to "ReportRole";
create policy "Company1GroupRoles" on "Security"."GroupRoles" to "ReportRole1" using (true);

alter table "Security"."GroupConflicts" enable row level security;
grant select on table "Security"."GroupConflicts" to "ReportRole";
create policy "Company1GroupConflicts" on "Security"."GroupConflicts" to "ReportRole1" using (true);

grant execute on function "Security"."GetAllUserGrants" (_Company bigint, _Code varchar(60)) to "ReportRole";

create or replace function "Security"."CreateReportUser" (company bigint) returns boolean as 
...

create or replace function "Security"."AlterReportUser" (company bigint) returns boolean as 
$$
begin
    execute replace
    (
    $r$
        create policy "Company$UserLevels" on "Security"."UserLevels" to "ReportRole$" using 
        ("Company" = $);
        create policy "Company$UserFunctions" on "Security"."UserFunctions" to "ReportRole$" using 
        ("Company" = $);
        create policy "Company$UserRoles" on "Security"."UserRoles" to "ReportRole$" using 
        ("Company" = $);
        create policy "Company$UserGroups" on "Security"."UserGroups" to "ReportRole$" using 
        ("Company" = $);
        create policy "Company$RoleFunctions" on "Security"."RoleFunctions" to "ReportRole$" using 
        ("Company" = $);
        create policy "Company$GroupFunctions" on "Security"."GroupFunctions" to "ReportRole$" using 
        ("Company" = $);
        create policy "Company$GroupRoles" on "Security"."GroupRoles" to "ReportRole$" using 
        ("Company" = $);
        create policy "Company$GroupConflicts" on "Security"."GroupConflicts" to "ReportRole$" using 
        ("Company" = $);
    $r$, 
    '$', company::varchar
    );

    return true;
end;
$$ language plpgsql;

select "Security"."AlterReportUser"("ID") from "Infrastructure"."Companies" where "ID" > 1;

drop function "Security"."AlterReportUser" (company bigint);


--da qui in poi va tenuto, sopra no
--delete from "Reports"."ReportSources";
insert into "Reports"."ReportSources" values
(
    'Audit trail',
    'select * from "Trail"."Records"'
);

insert into "Reports"."ReportSources" values
(
    'Prototypes',
    'select * from "CheckLists"."Prototypes"'
);

insert into "Reports"."ReportSources" values
(
    'Instances',
    'select * from "CheckLists"."Instances"'
);

insert into "Reports"."ReportSources" values
(
    'Users',
'
select 
    u."Company" "Company ID", c."Name" "Company Name",
    u."Code", u."Description" "Name And Surname", u."Email", u."CorporateID" "Corporate ID", u."Enabled", u."LastLoginOn" "Last Login On", 
    date_part(''day'', now() - u."LastLoginOn") "Number Of Days Since Last Login"
from 
    "Security"."Users" u
    inner join "Infrastructure"."Companies" c on u."Company" = c."ID"
order by "Company ID", "Code"
'
);

insert into "Reports"."ReportSources" values
(
    'Grants',
'
select 
    u."Company" "Company ID", c."Name" "Company Name",
    u."Code", u."Description" "Name And Surname", u."Email", u."CorporateID" "Corporate ID", u."Enabled", u."LastLoginOn" "Last Login On", 
    g."Group", g."Department" "Department ID", d."Name" "Department Name", g."PrototypeType" "Prototype Type ID", pt."Name" "Prototype Type Name", 
    g."Role", g."FunctionGroup" "Function Group", g."Function", (
        select "SignatureLevel" 
        from "Security"."UserLevels" ul 
        where 
            ul."Company" = u."Company" 
            and ul."User" = u."Code" 
            and (ul."Department" is null or ul."Department" = g."Department") 
            and (ul."PrototypeType" is null or ul."PrototypeType" = g."PrototypeType") 
            and not "SignatureLevel" is null 
        order by ul."PrototypeType"
        limit 1
    ) "Signature Level", (
        select "PrototypeWorkFlowLevel" 
        from "Security"."UserLevels" ul 
        where 
            ul."Company" = u."Company" 
            and ul."User" = u."Code" 
            and (ul."Department" is null or ul."Department" = g."Department") 
            and (ul."PrototypeType" is null or ul."PrototypeType" = g."PrototypeType") 
            and not "PrototypeWorkFlowLevel" is null 
        order by ul."PrototypeType"
        limit 1
    ) "Prototype Workflow Level", (
        select "InstanceWorkFlowLevel" 
        from "Security"."UserLevels" ul 
        where 
            ul."Company" = u."Company" 
            and ul."User" = u."Code" 
            and (ul."Department" is null or ul."Department" = g."Department") 
            and (ul."PrototypeType" is null or ul."PrototypeType" = g."PrototypeType") 
            and not "InstanceWorkFlowLevel" is null 
        order by ul."PrototypeType"
        limit 1
    ) "Instance Workflow Level"
from 
    "Security"."Users" u 
    inner join "Infrastructure"."Companies" c on u."Company" = c."ID"
    inner join lateral "Security"."GetAllUserGrants"(u."Company", u."Code") g on true 
    left join "Infrastructure"."Departments" d on g."Department" = d."ID" 
    left join "CheckLists"."PrototypeTypes" pt on g."PrototypeType" = pt."ID" 
order by u."Company", u."Code", g."Department", g."PrototypeType", g."Role", g."FunctionGroup", g."Function"
'
);

insert into "Reports"."ReportSources" values
(
    'Roles',
'
select 
    r."Company" "Company ID", c."Name" "Company Name",
    r."Code", 
    rf."FunctionGroup" "Function Group", rf."Function"
from 
    "Security"."Roles" r
    inner join "Infrastructure"."Companies" c on r."Company" = c."ID"
    left join "Security"."RoleFunctions" rf on r."Company" = rf."Company" and r."Code" = rf."Role"
order by r."Company", r."Code", rf."FunctionGroup", rf."Function"   
'
);

insert into "Reports"."ReportSources" values
(
    'Groups',
'
select 
    g."Company" "Company ID", c."Name" "Company Name",
    g."Code", 
    gf."Department" "Department ID", d."Name" "Department Name", gf."PrototypeType" "Prototype Type ID", pt."Name" "Prototype Type Name", 
    null "Role", gf."FunctionGroup" "Function Group", gf."Function"
from 
    "Security"."Groups" g
    inner join "Infrastructure"."Companies" c on g."Company" = c."ID"
    left join "Security"."GroupFunctions" gf on g."Company" = gf."Company" and g."Code" = gf."Group"
    left join "Infrastructure"."Departments" d on gf."Department" = d."ID" 
    left join "CheckLists"."PrototypeTypes" pt on gf."PrototypeType" = pt."ID" 
union 
select 
    g."Company" "Company ID", c."Name" "Company Name",
    g."Code", 
    gr."Department" "Department ID", d."Name" "Department Name", gr."PrototypeType" "Prototype Type ID", pt."Name" "Prototype Type Name", 
    gr."Role", null "Function Group", null "Function"
from 
    "Security"."Groups" g
    inner join "Infrastructure"."Companies" c on g."Company" = c."ID"
    left join "Security"."GroupRoles" gr on g."Company" = gr."Company" and g."Code" = gr."Group"
    left join "Infrastructure"."Departments" d on gr."Department" = d."ID" 
    left join "CheckLists"."PrototypeTypes" pt on gr."PrototypeType" = pt."ID" 
order by "Company ID", "Code", "Department ID", "Prototype Type ID", "Role", "Function Group", "Function"
'
);

insert into "Reports"."ReportSources" values
(
    'User conflicts',
'
select 
    u."Company" "Company ID", c."Name" "Company Name",
    u."Code", u."Description" "Name And Surname",
    case when gc."Group1" < gc."Group2" then gc."Group1" else gc."Group2" end "Group 1",
    case when gc."Group1" < gc."Group2" then gc."Group2" else gc."Group1" end "Group 2"
from 
    "Security"."Users" u 
    inner join "Infrastructure"."Companies" c on u."Company" = c."ID"
    inner join "Security"."GroupConflicts" gc on 
        u."Company" = gc."Company"
        and gc."Group1" in (select ug."Group" from "Security"."UserGroups" ug where ug."Company" = u."Company" and ug."User" = u."Code")
        and gc."Group2" in (select ug."Group" from "Security"."UserGroups" ug where ug."Company" = u."Company" and ug."User" = u."Code")
order by     
    u."Company", u."Code", "Group 1", "Group 2"
'
);

insert into "Reports"."ReportSources" values
(
    'Group conflicts',
'
select 
    g."Company" "Company ID", c."Name" "Company Name",
    g."Code", gc."Group2" "Conflicting Group"
from 
    "Security"."Groups" g
    inner join "Infrastructure"."Companies" c on g."Company" = c."ID"
    inner join "Security"."GroupConflicts" gc on g."Company" = gc."Company" and g."Code" = gc."Group1"
union
select 
    g."Company" "Company ID", c."Name" "Company Name",
    g."Code", gc."Group1"
from 
    "Security"."Groups" g
    inner join "Infrastructure"."Companies" c on g."Company" = c."ID"
    inner join "Security"."GroupConflicts" gc on g."Company" = gc."Company" and g."Code" = gc."Group2"
order by "Company ID", "Code", "Conflicting Group"
'
);