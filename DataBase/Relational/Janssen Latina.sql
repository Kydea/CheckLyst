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


insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") values
	--(currval('"Infrastructure"."Companies_ID_seq"'), 'LDAPHost', 'eu.jnj.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpHost', 'email-smtp.eu-west-1.amazonaws.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPort', '587'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpUsername', 'AKIAIEIVPEHAHBTTCRKA'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPassword', 'AmUlYueJDWpD0lJZrzgiZFjxpi+uGf8G3b8f9+IRFdz5'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'EmailSender', 'noreply@matematici-aws.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockMessage', '[NameAndSurname], <br /> il tuo account è stato disabilitato in quanto non hai effettuato nessun accesso negli ultimi 400 giorni.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockWarningMessage', '[NameAndSurname], <br /> il tuo account verrà disabilitato se non effettuerai un accesso al sistema.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SignatureMessage', '[NameAndSurname], <br /> hai un prototipo da firmare. Accedi a CheckList Manager per effettuare l\'operazione richiesta.');