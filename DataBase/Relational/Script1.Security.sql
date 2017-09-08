create schema "Security" owner to postgres;

create table "Security"."Users"
(
    "Company" bigint references "Infrastructure"."Companies",
	"Code" varchar (60),
    
    constraint "Users_PK" primary key ("Company", "Code"),
    
	"Description" varchar (100) not null,
	"AuthenticationCode" text, --json with connection information used to authenticate users with ldap or other company systems
	"Email" text,
	"Enabled" boolean not null default false,

    "Password" bytea,
	"PasswordChangeDate" timestamptz,

	"PIN" bytea,
	"PINChangeDate" timestamptz,

    "NumberOfAttempts" int not null default 0,

	"LastLoginOn" timestamptz,

	"CreatedBy" varchar (60),
	"CreatedOn" timestamptz not null default current_timestamp,
	"ModifiedBy" varchar (60),
	"ModifiedOn" timestamptz
);

insert into Security.Users (Code, Description, Password, Enabled, SettingsManager) values ('CheckListManager', 'System administrator', 0x5b852905cefc6d59524c45b8f642927b5459c2f622d40002466109e69dbccc36, 1, 1); --V4sc0 R0ss1, sha256
	
create table "Security"."PINHistory"
(
    "ID" bigserial primary key,

    "Company" bigint,
	"User" varchar (60),
    constraint "PINHistory_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,
	"PIN" bytea,

    "Date" timestamptz not null default current_timestamp
);

create index "Index_PINHistory_User" on "Security"."PINHistory"("Company", "User");

create table "Security"."PasswordHistory"
(
    "ID" bigserial primary key,

    "Company" bigint,
	"User" varchar (60),
    constraint "PasswordHistory_User" foreign key ("Company", "User") references "Security"."Users" on update cascade on delete cascade,
	"Password" bytea,

    "Date" timestamptz not null default current_timestamp
);

create index "Index_PasswordHistory_User" on "Security"."PasswordHistory"("Company", "User");

create or replace function "Security"."SetUserPIN"
(
	_company bigint,
	_user varchar(60),
	_pin bytea
) 
returns void as
$body$ 
declare 
	_count int;
begin

	delete from "Security"."PINHistory"
    where 
    	ph."Company" = _company 
        and ph."User" = _user
        and ph."Date" + interval '1' year < current_timestamp ;
	
	select _count = count(*) 
	from "Security"."PINHistory" ph 
	where 
    	ph."Company" = _company 
        and ph."User" = _user
		and ph."PIN" = _pin;

	if _count > 0 then
		raise 'This PIN is present in the last 5 PINs history of the past 365 days.' using errcode = 'Security.SetUserPIN.PINAlreadyUsed';
	end if;
    
	delete from "Security"."PINHistory"
    where "ID" in (
        select ph."ID"
        from "Security"."PINHistory" ph
        where
            ph."Company" = _company 
            and ph."User" = _user
        order by ph."ID" desc
        offset 5
    );

	update "Security"."Users" u
	set 
		u."PIN" = _pin,
		u."PINChangeDate" = current_timestamp
	where 
            ph2."Company" = _company 
            and ph2."User" = _user;

	insert into "Security"."PINHistory" ("Compnay", "User", "PIN", "Date") values (_company, _user, _pin, current_timestamp);
end 
$body$ language plpgsql;

create or replace function "Security"."SetUserPassword"
(
	_company bigint,
	_user varchar(60),
	_password bytea
) 
returns void as
$body$ 
declare 
	_count int;
begin

	delete from "Security"."PasswordHistory"
    where 
    	ph."Company" = _company 
        and ph."User" = _user
        and ph."Date" + interval '1' year < current_timestamp ;
	
	select _count = count(*) 
	from "Security"."PasswordHistory" ph 
	where 
    	ph."Company" = _company 
        and ph."User" = _user
		and ph."Password" = _password;

	if _count > 0 then
		raise 'This password is present in the last 5 passwords history of the past 365 days.' using errcode = 'Security.SetUserPassword.PasswordAlreadyUsed';
	end if;
    
	delete from "Security"."PasswordHistory"
    where "ID" in (
        select ph."ID"
        from "Security"."PasswordHistory" ph
        where
            ph."Company" = _company 
            and ph."User" = _user
        order by ph."ID" desc
        offset 5
    );

	update "Security"."Users" u
	set 
		u."Password" = _password,
		u."PasswordChangeDate" = current_timestamp
	where 
            ph2."Company" = _company 
            and ph2."User" = _user;

	insert into "Security"."PasswordHistory" ("Compnay", "User", "Password", "Date") values (_company, _user, _password, current_timestamp);
end 
$body$ language plpgsql;
