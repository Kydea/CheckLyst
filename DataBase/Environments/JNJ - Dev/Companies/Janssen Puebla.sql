insert into "Infrastructure"."TimeZone"  values ('Central Standard Time (Mexico)')

insert into "Infrastructure"."Companies"("Name", "TimeZone", "MaximumNumberOfPINFailure", "LoginFailedTimeout", "PINDaysLife", "MaximumNumberOfPasswordFailure", "PasswordDaysLife", "WarningUserInactivityDays", "MaximumUserInactivityDays") 
values ('Janssen Puebla', 'Central Standard Time (Mexico)', 5, 20, 90, 20, 365, 335, 365); --see http://momentjs.com/timezone/docs/#/data-loading/

insert into "Infrastructure"."Departments" ("Company", "Name") values (currval('"Infrastructure"."Companies_ID_seq"'), 'Automation (P)');
insert into "Infrastructure"."Departments" ("Company", "Name") values (currval('"Infrastructure"."Companies_ID_seq"'), 'Quality control (P)');
insert into "Infrastructure"."Departments" ("Company", "Name") values (currval('"Infrastructure"."Companies_ID_seq"'), 'Information technology (P)');

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
	(currval('"Infrastructure"."Companies_ID_seq"'), 'LDAPHost', 'la.jnj.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpHost', 'smtp.na.jnj.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPort', '25'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpUsername', ''),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPassword', ''),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'EmailSender', 'noreply-checklistmanager@its.jnj.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockMessage', '[User], <br /> tu cuenta en CheckLyst development evironment (Janssen Puebla) ha sido desactivada debido a un tiempo prolongado de inactividad.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockWarningMessage', '[User], <br /> tu cuenta en CheckLyst development evironment (Janssen Puebla) ha sido desactivada debido a un tiempo prolongado de inactividad.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SignatureMessage', '[User], <br /> tienes pendiente un workflow en Checklyst development environment (Janssen Puebla) para [CheckListType] [CheckListCode]..'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'InstanceMessage', '[User]<br/>La firma de aprobacion fue colocada en CheckLyst development environment (Janssen Puebla) para la instance [CheckListCode]'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'DecimalSeparator', '.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'RequireUserReason', 'true'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'PrototypesAttachmentMaxSizeBytes', '10485760'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'InstancesAttachmentMaxSizeBytes', '10485760');

  ---insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") values
--	(currval('"Infrastructure"."Companies_ID_seq"'), 'PrototypesFtpConnection', '{ "Host": "10.157.83.244", "Port": 21, "Folder": "/ChecklystPrototype_dev_gb",  "UserName": "FtpDev", "Password": "AA_Latina_01", "UsePassive": "true", "EnableSSL": "false" }');


  ---insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") values
---	(currval('"Infrastructure"."Companies_ID_seq"'), 'InstancesFtpConnection', '{ "Host": "10.157.83.244", "Port": 21, "Folder": "/ChecklystInstances_dev_gb",  "UserName": "FtpDev", "Password": "AA_Latina_01", "UsePassive": "true", "EnableSSL": "false" }');
  