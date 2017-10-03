insert into "Infrastructure"."Companies"("Name", "TimeZone") values ('Janssen Latina', 'Europe/Rome');

insert into "Infrastructure"."Departments" ("Company", "Name") values (currval('"Infrastructure"."Companies_ID_seq"'), 'Automation');
insert into "Infrastructure"."Departments" ("Company", "Name") values (currval('"Infrastructure"."Companies_ID_seq"'), 'Quality control');
insert into "Infrastructure"."Departments" ("Company", "Name") values (currval('"Infrastructure"."Companies_ID_seq"'), 'Information technology');

insert into "Security"."Users" ("Company", "Code", "Description", "Password", "Enabled") 
values (currval('"Infrastructure"."Companies_ID_seq"'), 'CheckListManager', 'System administrator', decode('5b852905cefc6d59524c45b8f642927b5459c2f622d40002466109e69dbccc36', 'hex'), true); --V4sc0 R0ss1, sha256

insert into "Security"."UserFunctions" ("Company", "User", "FunctionGroup", "Function")
values (currval('"Infrastructure"."Companies_ID_seq"'), 'CheckListManager', 'Company', 'Profiles');

insert into "Security"."UserFunctions" ("Company", "User", "FunctionGroup", "Function")
values (currval('"Infrastructure"."Companies_ID_seq"'), 'CheckListManager', 'Company', 'Data');