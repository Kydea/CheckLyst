select 
	ind.table_owner || '.' || ind.table_name as "TABLE",
	ind.index_name,
	listagg(ind_col.column_name, ',') within group(order by ind_col.column_position) as columns,
	ind.index_type,
	ind.uniqueness
from sys
	.all_indexes ind
	join sys.all_ind_columns ind_col on
		ind.owner = ind_col.index_owner
        and ind.index_name = ind_col.index_name
where 
	ind.table_owner not in 
	(
		'ANONYMOUS','CTXSYS','DBSNMP','EXFSYS', 'MDSYS', 'MGMT_VIEW','OLAPSYS','OWBSYS','ORDPLUGINS', 'ORDSYS', 
		'SI_INFORMTN_SCHEMA','SYS','SYSMAN','SYSTEM', 'TSMSYS','WK_TEST', 'WKPROXY','WMSYS','XDB','APEX_040000','APEX_040200',
		'DIP', 'FLOWS_30000','FLOWS_FILES','MDDATA', 'ORACLE_OCM', 'XS$NULL', 'SPATIAL_CSW_ADMIN_USR', 'SPATIAL_WFS_ADMIN_USR', 'PUBLIC',
		'LBACSYS', 'OUTLN', 'WKSYS', 'APEX_PUBLIC_USER'
	)
group by 
	ind.table_owner, ind.table_name, ind.index_name, ind.index_type, ind.uniqueness 
order by 
	ind.table_owner, ind.table_name;