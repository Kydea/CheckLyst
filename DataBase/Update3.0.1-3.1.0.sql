insert into "Trail"."Actions"
values 
	('User banned'),
	('User disabled'),
	('PIN banned'),
	('PIN disabled');

alter table "Reports"."ReportClauses" add "CaseSensitive" boolean not null default true;