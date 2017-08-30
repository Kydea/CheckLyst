insert into "Infrastructure"."Companies" values ('Janssen Latina');

insert into "Infrastructure"."Departments" ("Company"), values (currval('"Infrastructure"."Companies.ID"', 'Automation'));
insert into "Infrastructure"."Departments" ("Company"), values (currval('"Infrastructure"."Companies.ID"', 'Quality control'));
insert into "Infrastructure"."Departments" ("Company"), values (currval('"Infrastructure"."Companies.ID"', 'Information technology'));

insert into Security.Users (Compnay, Code, Description, Password, Enabled) 
values ('Janssen Latina', 'CheckListManager', 'System administrator', 0x5b852905cefc6d59524c45b8f642927b5459c2f622d40002466109e69dbccc36, true); --V4sc0 R0ss1, sha256