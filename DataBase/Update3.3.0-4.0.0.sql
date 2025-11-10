insert into "Configuration"."Settings" values ('License');

alter table "Infrastructure"."Companies" add "Online" boolean not null default true;
