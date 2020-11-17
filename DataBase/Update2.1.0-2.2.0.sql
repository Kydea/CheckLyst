--update Configuration - XXX.sql accordingly
insert into "Configuration"."Configurations" ("Code", "Value") values
	('NotificationsEmailAddress', <TBD>); 

alter table "CheckLists"."InstanceExpressionValues" add	"Notification" boolean not null default false;
alter table "CheckLists"."InstanceExpressionValues" add	"NotificationNote" varchar (100);
alter table "CheckLists"."InstanceExpressionValues" add	"NotificationEmail" varchar (100);
alter table "CheckLists"."InstanceExpressionValues" add	"Notified" boolean not null default false;
alter table "CheckLists"."InstanceExpressionValues" add	"NotificationDate" timestamptz;

create index "Index_InstanceExpressionValues_Notifications" on "CheckLists"."InstanceExpressionValues"("Value", "Instance", "Code") where "Notification" and not "Notified";

--take into the environment / companies scripts
insert into "Configuration"."Settings" ("Code") values
	('NotificationMessage');

--take into the environment / companies scripts
insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") 
select "ID", 'NotificationMessage', 'Notification for expression [Code]-[Description] (value [Value])<br /><br />[Note]<br /><br />[Link]'
from "Infrastructure"."Companies";