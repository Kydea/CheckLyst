create schema "Security";

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

    "Date" timestamptz not null default current_timestamp
);

create index "Index_PasswordHistory_User" on "Security"."PasswordHistory"("Company", "User");

create procedure SetUserPIN 
(
	@User varchar(60),
	@PIN varbinary (64)
) as
begin try
	declare @Count int;

	delete from Security.PINHistory where dateadd(day, 365, Date) < getutcdate() and [User] = @User;
	
	select @Count = count(*) 
	from Security.PINHistory ph 
	where 
		ph.[User] = @User 
		and ph.PIN = @PIN;

	if @Count > 0 raiserror ('This PIN is present in the last 5 PINs history of the past 365 days.', 16, 100);

	select @Count = count(*) from Security.PINHistory ph where ph.[User] = @User;
	if @Count = 5 delete from Security.PINHistory where Id = (select top 1 ph.Id from Security.PINHistory ph where ph.[User] = @User order by ph.Id);

	update Security.Users 
	set 
		PIN = @PIN,
		PINChangeDate = getutcdate()
	where Code = @User;
	insert into Security.PINHistory ([User], PIN, Date) values (@User, @PIN, getutcdate());
end try
begin catch
	declare @Error varchar (500) = error_message();
	raiserror ('Error %s', 16, 100, @Error);
end catch	

create procedure SetUserPassword 
...


go

create table Security.Functions
(
	Code varchar (20) primary key,
	Description varchar (100) not null
);

go

insert into Security.Functions values
('Administrator', 'Can define user profile and external sources'),
('PrototypeApprover', 'Can approve/reject prototype versions'),
('PrototypeDesigner', 'Can create prototype versions'),
('InstanceCreator', 'Can create intances'),
('InstanceOperator', 'Can use intance manager with existing instances'),
('InstanceApprover', 'Can approve instances'),
('InstanceViewer', 'Can see instances'),
('InstanceDeletion', 'Can delete instances'),
('SignatureLevel', ''),
('LockManager', ''),
('SignatureManager', 'Can revert electronic signartures');

go
