create schema "Configuration";

create table "Configuration"."Configurations"
(
	"Code" varchar (100),
	
	constraint "Configurations_PK" primary key ("Code"),
	
	"Value" varchar (500) not null,

	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedOn" timestamptz
);

insert into "Configuration"."Configurations" ("Code", "Value") values
	('RedisHost', 'redis-01.clm.internal'),
	('RedisPort', '6379'),
	('SqsBaseUrl', 'https://sqs.eu-west-1.amazonaws.com/'),
	('ApiBaseUrl', 'http://api.clm.internal/'),
	('InstanceCloseTimeout', '4320'); --3 days, expressed in minutes


create table "Configuration"."Settings"
(
	"Code" varchar (100),
	constraint "Settings_PK" primary key ("Code"),

	"Description" text,
	"DefaultValue" text,

	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedOn" timestamptz
);

insert into "Configuration"."Settings" ("Code") values
	('SmtpHost'),
	('SmtpPort'),
	('SmtpUsername'),
	('SmtpPassword'),
	('EmailSender'),
	('UserBlockMessage'),
	('UserBlockWarningMessage'),
	('SignatureMessage');

create table "Configuration"."SettingValues"
(
	"ID" bigserial primary key,

	"Company" bigint not null references "Infrastructure"."Companies",
	"Setting" varchar (100) not null references "Configuration"."Settings",

	"Value" text not null,

	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedOn" timestamptz
);

create unique index "Index_SettingValues_Company_Setting" on "Configuration"."SettingValues" ("Company", "Setting");

insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") values
	(1, 'SmtpHost', 'email-smtp.eu-west-1.amazonaws.com'),
	(1, 'SmtpPort', '587'),
	(1, 'SmtpUsername', 'AKIAIEIVPEHAHBTTCRKA'),
	(1, 'SmtpPassword', 'AmUlYueJDWpD0lJZrzgiZFjxpi+uGf8G3b8f9+IRFdz5'),
	(1, 'EmailSender', 'noreply@matematici-aws.com'),
	(1, 'UserBlockMessage', '[NameAndSurname], <br /> il tuo account è stato disabilitato in quanto non hai effettuato nessun accesso negli ultimi 400 giorni.'),
	(1, 'UserBlockWarningMessage', '[NameAndSurname], <br /> il tuo account verrà disabilitato se non effettuerai un accesso al sistema.'),
	(1, 'SignatureMessage', '[NameAndSurname], <br /> hai un prototipo da firmare. Accedi a CheckList Manager per effettuare l\'operazione richiesta.'),
	(2, 'SmtpHost', 'smtp.eu.jnj.com'),
	(2, 'SmtpPort', '25'),
	(2, 'SmtpUsername', ''),
	(2, 'SmtpPassword', ''),
	(2, 'EmailSender', 'noreply-checklistmanager@its.jnj.com'),
	(2, 'UserBlockMessage', '[NameAndSurname], <br /> il tuo account è stato disabilitato in quanto non hai effettuato nessun accesso negli ultimi 400 giorni.'),
	(2, 'UserBlockWarningMessage', '[NameAndSurname], <br /> il tuo account verrà disabilitato se non effettuerai un accesso al sistema.'),
	(2, 'SignatureMessage', '[NameAndSurname], <br /> hai un prototipo da firmare. Accedi a CheckList Manager per effettuare l\'operazione richiesta.');
