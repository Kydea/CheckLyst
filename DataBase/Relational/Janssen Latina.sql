insert into "Infrastructure"."Companies"("Name", "TimeZone") values ('Janssen Latina', 'Europe/Rome'); --see http://momentjs.com/timezone/docs/#/data-loading/

insert into "Infrastructure"."Departments" ("Company", "Name") values (currval('"Infrastructure"."Companies_ID_seq"'), 'Automation');
insert into "Infrastructure"."Departments" ("Company", "Name") values (currval('"Infrastructure"."Companies_ID_seq"'), 'Quality control');
insert into "Infrastructure"."Departments" ("Company", "Name") values (currval('"Infrastructure"."Companies_ID_seq"'), 'Information technology');

insert into "Security"."Users" ("Company", "Code", "Description", "Password", "PIN", "Enabled") 
values (
	currval('"Infrastructure"."Companies_ID_seq"'), 
	'CheckListManager', 
	'System administrator', 
	decode('5b852905cefc6d59524c45b8f642927b5459c2f622d40002466109e69dbccc36', 'hex'), --V4sc0 R0ss1, sha256
	decode('8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'hex'), --123456, sha256
	true
); 

insert into "Security"."UserFunctions" ("Company", "User", "FunctionGroup", "Function")
values (currval('"Infrastructure"."Companies_ID_seq"'), 'CheckListManager', 'Users', 'View');

insert into "Security"."UserFunctions" ("Company", "User", "FunctionGroup", "Function")
values (currval('"Infrastructure"."Companies_ID_seq"'), 'CheckListManager', 'Users', 'Edit');

