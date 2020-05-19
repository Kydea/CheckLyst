--update Configuration - XXX.sql accordingly, the url must be specified without final slash

insert into "Configuration"."Configurations" ("Code", "Value") values
	('NotificationsEmailAddress', <TBD>); 

alter table "CheckLists"."InstanceExpressionValues" add	"Notification" boolean not null default false;
alter table "CheckLists"."InstanceExpressionValues" add	"NotificationNote" varchar (100);
alter table "CheckLists"."InstanceExpressionValues" add	"NotificationEmail" varchar (100);
alter table "CheckLists"."InstanceExpressionValues" add	"Notified" boolean not null default false;

create index "Index_InstanceExpressionValues_Notifications" on "CheckLists"."InstanceExpressionValues"("Instance", "Code", "Value") where "Notification" and not "Notified";


insert into "Configuration"."Settings" ("Code") values
	('NotificationMessage');

insert into "Configuration"."SettingValues" ("Company", "Setting", "Value") 
select "ID", 'NotificationMessage', 'Notification for expression [Code]-[Description] (value [Value])<br /><br />[Note]'
from "Infrastructure"."Companies";
