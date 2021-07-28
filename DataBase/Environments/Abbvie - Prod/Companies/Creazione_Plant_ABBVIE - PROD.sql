insert into "Infrastructure"."Companies"("Name", "TimeZone", "MaximumNumberOfPINFailure", "LoginFailedTimeout", "PINDaysLife", "MaximumNumberOfPasswordFailure", "PasswordDaysLife", "WarningUserInactivityDays", "MaximumUserInactivityDays") 
values ('ABBVIE - Campoverde', 'W. Europe Standard Time', 5, 30, 90, 20, 365, 335, 365); --see http://momentjs.com/timezone/docs/#/data-loading/

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
	(currval('"Infrastructure"."Companies_ID_seq"'), 'LDAPHost', 'ITDC103'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpHost', 'SMTP.ABBVIE.COM'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPort', '25'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpUsername', ''),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPassword', ''),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'EmailSender', 'noreply-checklistcv@abbvie.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockMessage', '[User], <br /> your account in CheckLyst environment has been disabled because you did not login for too much time.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockWarningMessage', '[User], <br /> your account in CheckLyst environment will soon be disabled if you do not login.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SignatureMessage', '[User], <br /> you have a pending workflow in CheckLyst  environment for the [CheckListType] [CheckListCode].'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'InstanceMessage', '[User]<br/>A signature has been issued in CheckLyst environment for the instance [CheckListCode]'),
    (currval('"Infrastructure"."Companies_ID_seq"'), 'NotificationMessage', 'Notification for expression [Code]-[Description] (value [Value])<br /><br />[Note]<br /><br />[Link]'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'DecimalSeparator', ','),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'RequireUserReason', 'True'),                                                                                                             
	(currval('"Infrastructure"."Companies_ID_seq"'), 'PrototypesAttachmentMaxSizeBytes', '10485760'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'InstancesAttachmentMaxSizeBytes', '10485760');



