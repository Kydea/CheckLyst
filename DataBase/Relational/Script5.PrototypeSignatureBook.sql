create table CheckLists.PrototypeSignatureBookActions
(
	Code varchar (50) primary key
);

insert into CheckLists.PrototypeSignatureBookActions values ('Prototype – state to Approved/Active');
insert into CheckLists.PrototypeSignatureBookActions values ('Prototype – state to Obsolete');

go

create table CheckLists.PrototypeSignatureBook
(
	Id int identity primary key,

	Company varchar (50) not null,
	Department varchar (50) not null,
	PrototypeType varchar (50) not null,
	constraint FK_PrototypeSignatureBook_PrototypeType foreign key (Company, Department, PrototypeType) references CheckLists.PrototypeTypes on update cascade,

	Action varchar (50) foreign key (Action) references CheckLists.PrototypeSignatureBookActions,

	Step int not null,
	SignatureLevel int not null,
	SignatureReason varchar (50) not null
);

go

create index Index_PrototypeSignatureBook_Action on CheckLists.PrototypeSignatureBook(Company, Department, PrototypeType, Action);

go

create table CheckLists.PrototypeSignatures
(
	Id int identity primary key,

	Prototype varchar (50) not null,
	Version int not null,
	constraint FK_PrototypeSignatures_Version foreign key (Prototype, Version) references CheckLists.Versions on update cascade,

	Action varchar (50) not null foreign key (Action) references CheckLists.PrototypeSignatureBookActions,

	Step int not null,
	SignatureLevel int not null,
	SignatureReason varchar (50) not null,
	Comment varchar(max),

	[User] varchar (60) not null references Security.Users,
	Date datetime not null default getdate(), --Local
	UTCDate datetime not null default getutcdate(), --UTC

	Approved bit not null default 0 --1: approved, 0: rejected
);

go

create index Index_PrototypeSignatures_Action on CheckLists.PrototypeSignatures(Prototype, Version, Action);

go

create table CheckLists.History
(
	Id int identity primary key,

	Prototype varchar (50) not null,
	Version int not null,
	Year int,
	Number int,

	Date datetime not null default getdate(), --Local
	UTCDate datetime not null default getutcdate(), --UTC

	State varchar (100) not null,
	[User] varchar (60) not null,

	SignatureStep int,
	SignatureLevel int,
	SignatureReason varchar (50),
	SignatureComment varchar(max),
	SignatureResult bit --1: approved, 0: rejected
);

go

create index Index_History_CheckList on CheckLists.History(Prototype, Version, Year, Number, UtcDate);

go