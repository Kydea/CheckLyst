alter table "Infrastructure"."Companies" add "Online" boolean not null default true;

insert into "Configuration"."Settings" values ('LicenseExpiration');
insert into "Configuration"."Settings" values ('LicenseCompaniesCount');
insert into "Configuration"."Settings" values ('LicensePrototypeTypesCount');
insert into "Configuration"."Settings" values ('LicensePrototypesCount');
insert into "Configuration"."Settings" values ('LicenseUsersCount');
insert into "Configuration"."Settings" values ('LicenseSignature');
