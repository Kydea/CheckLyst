create table Security.UserFunctions
(
	[User] varchar (60) references Security.Users on update cascade on delete cascade,

	Company varchar (50),
	Department varchar (50),
	PrototypeType varchar (50),
	SignatureLevel int,
	constraint FK_UserFunctions_PrototypeType foreign key (Company, Department, PrototypeType) references CheckLists.PrototypeTypes on update cascade on delete cascade,

	[Function] varchar (20),
	constraint FK_UserFunctions_Profile foreign key ([Function]) references Security.Functions on update cascade,

	constraint PK_UserFunctions primary key ([User], PrototypeType, Company, Department, [Function]),

	CreatedBy varchar (60) references Security.Users,
	CreatedOn datetime not null default getdate(), 
	UTCCreatedOn datetime,
	ModifiedBy varchar (60) references Security.Users,
	ModifiedOn datetime,
	UTCModifiedOn datetime
);

go