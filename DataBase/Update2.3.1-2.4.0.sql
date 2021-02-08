alter table "CheckLists"."InstanceExpressionValues" alter set default null;
alter table "CheckLists"."InstanceExpressionValues" alter "Unique" type int using case when "Unique" then 1 else 0 end;

--Script3.CheckLists.sql > create or replace function "CheckLists"."ValidateUniqueExpressions"...






--RICONTROLLARE DA QUI IN POI

--Script8.Documents.sql

alter table "WorkFlows"."WorkFlowApplications" add "Document" bigint;
alter table "WorkFlows"."WorkFlowApplications" add "DocumentVersion" bigint;
alter table "WorkFlows"."WorkFlowApplications" add constraint "WorkFlowApplications_DocumentVersion" foreign key ("Document", "DocumentVersopn") references "Documents"."DocumentVersions" on delete cascade;

alter table "WorkFlows"."WorkFlowApplications" rename column "Version" to "PrototypeVersion";

--manually delete the following unnamed check from "WorkFlows"."WorkFlowApplications" check("Prototype" is null and "Version" is null or "Instance" is null)

alter table "WorkFlows"."WorkFlowApplications" add
	constraint Check_WorkFlowApplications_Target check
	(
		not "Prototype" is null and "Instance" is null and "Document" is null 
		or "Prototype" is null and not "Instance" is null and "Document" is null 
		or "Prototype" is null and "Instance" is null and not "Document" is null 
	);

alter table "WorkFlows"."WorkFlowApplications" add
	constraint Check_WorkFlowApplications_PrototypeVersion check("Prototype" is null and "PrototypeVersion" is null or not "Prototype" is null and not "PrototypeVersion" is null),

alter table "WorkFlows"."WorkFlowApplications" add
	constraint Check_WorkFlowApplications_DocumentVersion check("Document" is null and "DocumentVersion" is null or not "Document" is null and not "DocumentVersion" is null),
