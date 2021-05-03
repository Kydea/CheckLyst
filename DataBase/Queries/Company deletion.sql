do 
$$ 

declare	
	CompanyID int := 10; --Provide the ID of the company to delete
	CompanyName varchar(50) := 'Janssen Shaffhausen'; --Provide the Name of the company to delete

begin
	select "ID" into CompanyID from "Infrastructure"."Companies" where "ID" = CompanyID and "Name" = CompanyName;
	if CompanyID is null then
		raise exception  'Company not found' using hint = 'Please check the CompanyID and the CompnanyName provided';
	end if;
	
	delete from "Security"."Users" where "Company" = CompanyID;
	delete from "Configuration"."SettingValues" where "Company" = CompanyID;
	delete from "Infrastructure"."Companies" where "ID" = CompanyID;

	raise notice 'company %-% deleted successfully', CompanyID, CompanyName;
end

$$


/*

soft company deletion (works only on new company with no user)

delete from "Configuration"."SettingValues" where "Company" = (select "ID" from "Infrastructure"."Companies" where "Name" = 'test');
delete from "Security"."UserSessions" where "Company" = (select "ID" from "Infrastructure"."Companies" where "Name" = 'test');
delete from "Security"."Users" where "Company" = (select "ID" from "Infrastructure"."Companies" where "Name" = 'test');
delete from "Infrastructure"."Companies" where "Name" = 'test';

*/