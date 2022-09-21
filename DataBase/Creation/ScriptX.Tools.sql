alter table "CheckLists"."Instances" add column "ToolState" varchar(50);
create schema "Tools";

create table "Tools"."InstancesAnalysis"
(
    "State" varchar(100),
    "RecordCount" int,
    "CurrentRecordIndex" int,
    "Ok" int,
    "Ko" int,
    "InitialTime" timestamptz,
    "FinalTime" timestamptz
);

insert into "Tools"."InstancesAnalysis" ("State") values ('Stopped');

create table "Tools"."InstancesAnalysisLog"
(
    "Key" text,
    "Message" text
);

create or replace function "Tools"."InstancesAnalysisContinue"()
returns int as
$$ 
begin
    update "Tools"."InstancesAnalysis" 
    set 
        "State" = 'Started', 
        "InitialTime" = current_timestamp,
        "FinalTime" = null;
        
    return 0;
end
$$
language plpgsql;

create or replace function "Tools"."InstancesAnalysisRestart"()
returns int as
$$ 
begin
    update "Tools"."InstancesAnalysis" 
    set 
        "State" = 'Started', 
        "InitialTime" = current_timestamp,
        "FinalTime" = null,
        "RecordCount" = (select count(*) from "CheckLists"."Instances"),
        "CurrentRecordIndex" = 0,
        "Ok" = 0,
        "Ko" = 0;
        
    update "CheckLists"."Instances" 
    set 
        "ToolState" = null; 
    delete from "Tools"."InstancesAnalysisLog";

    return 0;
end
$$
language plpgsql;

create or replace function "Tools"."InstancesAnalysisStop"()
returns int as
$$ 
begin
    update "Tools"."InstancesAnalysis" 
    set 
        "State" = 'Stopped', 
        "FinalTime" = current_timestamp; 

    return 0;
end
$$
language plpgsql;

create or replace function "Tools"."InstancesAnalysisOk"
(
    "_Instance" bigint
)
returns int as
$$ 
begin
    update "Tools"."InstancesAnalysis" 
    set 
        "CurrentRecordIndex" = "CurrentRecordIndex" + 1,
        "Ok" = "Ok" + 1;

    update "CheckLists"."Instances" set "ToolState" = 'OK' where "ID" = "_Instance";
    return 0;
end
$$
language plpgsql;

create or replace function "Tools"."InstancesAnalysisError"
(
    "_Key" text,
    "_Message" text,
    "_Instance" bigint
)
returns int as
$$ 
begin
    update "Tools"."InstancesAnalysis" 
    set 
        "CurrentRecordIndex" = "CurrentRecordIndex" + 1,
        "Ko" = "Ko" + 1;

    insert into "Tools"."InstancesAnalysisLog" ("Key", "Message") values ("_Key", "_Message");

    update "CheckLists"."Instances" set "ToolState" = 'KO' where "ID" = "_Instance";

    return 0;
end
$$
language plpgsql;

--alter table "CheckLists"."Instances" drop column "ToolState" 

--http://localhost/API/MigrationTools/Pages/InstancesAnalysis/