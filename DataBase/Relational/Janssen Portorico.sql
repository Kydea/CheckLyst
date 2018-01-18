insert into "Infrastructure"."Companies"("Name", "TimeZone") values ('Janssen Portorico', 'America/Puerto_Rico'); --see http://momentjs.com/timezone/docs/#/data-loading/

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
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpHost', 'smtp.eu.jnj.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPort', '25'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpUsername', ''),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SmtpPassword', ''),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'EmailSender', 'noreply-checklistmanager@its.jnj.com'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockMessage', '[NameAndSurname], <br /> il tuo account è stato disabilitato in quanto non hai effettuato nessun accesso negli ultimi 400 giorni.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'UserBlockWarningMessage', '[NameAndSurname], <br /> il tuo account verrà disabilitato se non effettuerai un accesso al sistema.'),
	(currval('"Infrastructure"."Companies_ID_seq"'), 'SignatureMessage', '[NameAndSurname], <br /> hai un prototipo da firmare. Accedi a CheckList Manager per effettuare l\'operazione richiesta.');