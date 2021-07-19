insert into "Reports"."ReportSources" values
(
    'Audit trail',
'
select
    c."ID" "Company ID", c."Name" "Company Name",
    r."ID" "ID", r."Date" "Date",
    r."User" "User", u."Description" "Name and Surname",
    r."Action" "Action", r."Information" "Information", 
    r."CheckListCode" "CheckList Code", r."CheckListID" "CheckList ID"
from
    "Trail"."Records" r
    inner join "Infrastructure"."Companies" c on r."Company" = c."ID"
    left join "Security"."Users" u on r."Company" = u."Company" and r."User" = u."Code"
order by 
    c."ID", r."ID"
'
);

insert into "Reports"."ReportSources" values
(
    'Prototypes',
'
select
    c."ID" "Company ID", c."Name" "Company Name",  
    d."ID" "Department ID", d."Name" "Department Name", 
    pt."ID" "PrototypeType ID", pt."Name" "PrototypeType Name", 
    p."ID" "Prototype ID", p."Name" "Prototype Name", p."Description" "Prototype Description",
    pv."Version" "Prototype Version", pv."VersionState" "Prototype Version State",
    pv."ValidFrom" "Valid From", pv."ValidUntil" "Valid Until",
    pv."CreatedBy" "Prototype Created By", pv."CreatedOn" "Prototype Created On",
    pv."ModifiedBy" "Prototype Modified By", pv."ModifiedOn" "Prototype Modified On"
from
    "CheckLists"."Prototypes" p
    inner join "Infrastructure"."Companies" c on p."Company" = c."ID"
    inner join "Infrastructure"."Departments" d on p."Department" = d."ID"
    inner join "CheckLists"."PrototypeTypes" pt on p."PrototypeType" = pt."ID"
    inner join "CheckLists"."PrototypeVersions" pv on p."ID" = pv."Prototype"
order by 
    c."ID", d."Name", pt."Name", p."Name", pv."Version"
'
);

insert into "Reports"."ReportSources" values
(
    'Instances',
'
select 
    c."ID" "Company ID", c."Name" "Company Name",  
    d."ID" "Department ID", d."Name" "Department Name", 
    pt."ID" "PrototypeType ID", pt."Name" "PrototypeType Name", 
    p."ID" "Prototype ID", p."Name" "Prototype Name", p."Description" "Prototype Description",
    i."PrototypeName" || ''.'' || i."Version" || ''.'' || i."Year" || ''.'' || i."Number" as "Instance Code",
    i."ID" "Instance ID", i."InstanceState" "Instance State", 
    i."Description" "Instance Description", i."FileName" "Instance File Name",
    i."CreatedBy" "Instance Created By", i."CreatedOn" "Instance Created On",
    i."ModifiedBy" "Instance Modified By", i."ModifiedOn" "Instance Modified On",
    id."ItemsByLabel", id."ItemsByCode", id."Expressions"
from 
    "CheckLists"."Instances" i
    inner join "Infrastructure"."Companies" c on i."Company" = c."ID"
    inner join "CheckLists"."Prototypes" p on i."Prototype" = p."ID"
    inner join "Infrastructure"."Departments" d on p."Department" = d."ID"
    inner join "CheckLists"."PrototypeTypes" pt on p."PrototypeType" = pt."ID"
    left join "CheckLists"."InstanceData" id on i."ID" = id."Instance"
order by 
    c."ID", d."Name", pt."Name", i."Prototype", i."Version", i."ID"
'
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
    u."Code" "User Code", u."Description" "Name And Surname", u."Email", u."CorporateID" "Corporate ID", u."Enabled", u."LastLoginOn" "Last Login On", 
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
    g."Code" "Group Code", g."Description" "Group Description",
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
    g."Code" "Group Code", g."Description" "Group Description",
    gr."Department" "Department ID", d."Name" "Department Name", gr."PrototypeType" "Prototype Type ID", pt."Name" "Prototype Type Name", 
    gr."Role", null "Function Group", null "Function"
from 
    "Security"."Groups" g
    inner join "Infrastructure"."Companies" c on g."Company" = c."ID"
    left join "Security"."GroupRoles" gr on g."Company" = gr."Company" and g."Code" = gr."Group"
    left join "Infrastructure"."Departments" d on gr."Department" = d."ID" 
    left join "CheckLists"."PrototypeTypes" pt on gr."PrototypeType" = pt."ID" 
order by "Company ID", "Group Code", "Department ID", "Prototype Type ID", "Role", "Function Group", "Function"
'
);

insert into "Reports"."ReportSources" values
(
    'User conflicts',
'
select 
    u."Company" "Company ID", c."Name" "Company Name",
    u."Code" "User Code", u."Description" "Name And Surname", u."CorporateID" "Corporate ID", u."Enabled",
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
    g."Code" "Group Code", gc."Group2" "Conflicting Group"
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
order by "Company ID", "Group Code", "Conflicting Group"
'
);