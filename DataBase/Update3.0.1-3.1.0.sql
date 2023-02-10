insert into "Trail"."Actions"
values 
	('User banned'),
	('User disabled'),
	('PIN banned'),
	('PIN disabled');

create or replace function "Security"."Users_Count_Limit_Trigger"()
returns trigger as
$body$
begin
	--current number: select count(*) from "Security"."Users" where "Enabled";
	if (select count(*) from "Security"."Users" where "Enabled") > ? then
		raise exception 'The number of users exceeds the the license';
	end if;
	return new;
end;
$body$
language plpgsql;

create trigger "Licensed users check"
after insert or update on "Security"."Users"
for each row execute procedure "Security"."Users_Count_Limit_Trigger"();
