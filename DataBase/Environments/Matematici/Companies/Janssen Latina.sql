insert into "Infrastructure"."Companies"("Name", "TimeZone", "MaximumNumberOfPINFailure", "LoginFailedTimeout", "PINDaysLife", "MaximumNumberOfPasswordFailure", "PasswordDaysLife", "WarningUserInactivityDays", "MaximumUserInactivityDays") 
values ('Janssen Latina', 'Europe/Rome', 5, 20, 365, 20, 365, 335, 365); --see http://momentjs.com/timezone/docs/#/data-loading/

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

insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") values
	--(currval('"Infrastructure"."Companies_ID_seq"'), 'LDAPHost', 'eu.jnj.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'InstancesFtpConnection', '{ "Host": "www.ruggerogiovannelli.org", "Port": 21, "Folder": "ruggerogiovannelli.org/Data/P", "UserName": "2348977@aruba.it", "Password": "-", "UsePassive": "true", "EnableSSL": "false" }'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'PrototypesFtpConnection', '{ "Host": "www.ruggerogiovannelli.org", "Port": 21, "Folder": "ruggerogiovannelli.org/Data/I", "UserName": "2348977@aruba.it", "Password": "-", "UsePassive": "true", "EnableSSL": "false" }'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpHost', 'email-smtp.eu-west-1.amazonaws.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPort', '587'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpUsername', 'AKIAIEIVPEHAHBTTCRKA'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPassword', 'AmUlYueJDWpD0lJZrzgiZFjxpi+uGf8G3b8f9+IRFdz5'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'EmailSender', 'noreply@matematici-aws.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockMessage', '[User], <br /> your account has been disabled because you did not login for too much time.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockWarningMessage', '[User], <br /> your account will soon be disabled if you do not login.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SignatureMessage', '[User], <br /> you have a pending workflow for the [CheckListType] [CheckListCode].'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'DecimalSeparator', '.');