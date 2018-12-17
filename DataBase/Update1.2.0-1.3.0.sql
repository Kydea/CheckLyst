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
	
--for each company id
insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") values
	(id, 'InstancesFtpConnection', '{ "Host": "www.ruggerogiovannelli.org", "Port": 21, "Folder": "ruggerogiovannelli.org/Data/P", "UserName": "2348977@aruba.it", "Password": "oeof5sxc", "UsePassive": "true", "EnableSSL": "false" }'),
	(id, 'PrototypesFtpConnection', '{ "Host": "www.ruggerogiovannelli.org", "Port": 21, "Folder": "ruggerogiovannelli.org/Data/I", "UserName": "2348977@aruba.it", "Password": "oeof5sxc", "UsePassive": "true", "EnableSSL": "false" }');
