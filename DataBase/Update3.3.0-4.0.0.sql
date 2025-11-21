alter table "Infrastructure"."Companies" add "Online" boolean not null default true;
alter table "Security"."Users" add "TimeZone" varchar (100) references "Infrastructure"."TimeZones";

insert into "Configuration"."Settings" values ('LicenseExpiration');
insert into "Configuration"."Settings" values ('LicenseCompaniesCount');
insert into "Configuration"."Settings" values ('LicensePrototypeTypesCount');
insert into "Configuration"."Settings" values ('LicensePrototypesCount');
insert into "Configuration"."Settings" values ('LicenseUsersCount');
insert into "Configuration"."Settings" values ('LicenseSignature');
