--Project Nova
--Author Massimiliano Carosi
--Company Matematici
--Date 2020-10-14

--parameters
declare
	--sku prototype
	@SKUPrototype varchar(max) = 'Prototipo_Ricetta_Antares_NEW',

	--query type
	@OnlyConfirmed varchar(1) = 'N', --Y/N

	--header fields
	@ClusterLabel varchar(max) = 'Cluster di riferimento',
	@ProdottoLabel varchar(max) = 'Codice prodotto',
	@LineaLabel varchar(max) = 'Linea',
	@RicettaBlisterLabelCluster varchar(max) = 'Ricetta blister',
	@RicettaBilancia1Cluster varchar(max) = 'Ricetta bilancia 1',
	@RicettaBilancia2Cluster varchar(max) = 'Ricetta bilancia 2',
	@TipoLavorazioneLabel varchar(max) = 'Tipo lavorazione',
	@ApprovazioneIntermediaClusterLabel varchar(max) = 'Approvazione intermedia',
	@RicettaBlisterLabelSKU varchar(max) = 'Ricetta blister',
	@RicettaBilancia1SKU varchar(max) = 'Ricetta bilancia 1',
	@RicettaBilancia2SKU varchar(max) = 'Ricetta bilancia 2',
	@ApprovazioneIntermediaSKULabel varchar(max) = 'Approvazione intermedia'	


--do not modify anything under this row

declare @Query varchar(max)

set @Query = '
	select 
		itp."Value" "CODICE_PRODOTTO", itl."Value" "LINEA", 

		cluster."ID" "ID_CLUSTER", cluster."InstanceState" "STATO_CLUSTER", cluster."Description" "DESCRIZIONE_CLUSTER",
		SKU."ID" "ID_SKU", sku."InstanceState" "STATO_SKU", sku."Description" "DESCRIZIONE_SKU",

		itrbc."Value" "RICETTA_BLISTER_CLUSTER", itr1c."Value" "RICETTA_BILANCIA_1_CLUSTER", itr2c."Value" "RICETTA_BILANCIA_2_CLUSTER",
		ittlc."Value" "TIPO_LAVORAZIONE", itais."Value" "APPROVAZIONE_INTERMEDIA_CLUSTER",
		itrbs."Value" "RICETTA_BLISTER_SKU", itr1s."Value" "RICETTA_BILANCIA_1_SKU", itr2s."Value" "RICETTA_BILANCIA_2_SKU",
		itais."Value" "APPROVAZIONE_INTERMEDIA_SKU"
	from 
		"CheckLists"."Instances" sku
		inner join "CheckLists"."InstanceTags" link on sku."ID" = link."Instance" and link."Tag" = ''' + @ClusterLabel + '''
		inner join "CheckLists"."Instances" cluster on cast(link."Value" as int) = cluster."ID"
		inner join "CheckLists"."InstanceTags" itp on sku."ID" = itp."Instance" and itp."Tag" = ''' + @ProdottoLabel + '''
		inner join "CheckLists"."InstanceTags" itl on sku."ID" = itl."Instance" and itl."Tag" = ''' + @LineaLabel + '''

		left join "CheckLists"."InstanceTags" itrbc on cluster."ID" = itrbc."Instance" and itrbc."Tag" = ''' + @RicettaBlisterLabelCluster + '''
		left join "CheckLists"."InstanceTags" itr1c on cluster."ID" = itr1c."Instance" and itr1c."Tag" = ''' + @RicettaBilancia1Cluster + '''
		left join "CheckLists"."InstanceTags" itr2c on cluster."ID" = itr2c."Instance" and itr2c."Tag" = ''' + @RicettaBilancia2Cluster + '''
		left join "CheckLists"."InstanceTags" ittlc on cluster."ID" = ittlc."Instance" and ittlc."Tag" = ''' + @TipoLavorazioneLabel + '''
		left join "CheckLists"."InstanceTags" itaic on cluster."ID" = itaic."Instance" and itaic."Tag" = ''' + @ApprovazioneIntermediaClusterLabel + '''

		left join "CheckLists"."InstanceTags" itrbs on sku."ID" = itrbs."Instance" and itrbs."Tag" = ''' + @RicettaBlisterLabelSKU + '''
		left join "CheckLists"."InstanceTags" itr1s on sku."ID" = itr1s."Instance" and itr1s."Tag" = ''' + @RicettaBilancia1SKU + '''
		left join "CheckLists"."InstanceTags" itr2s on sku."ID" = itr2s."Instance" and itr2s."Tag" = ''' + @RicettaBilancia2SKU + '''
		left join "CheckLists"."InstanceTags" itais on sku."ID" = itais."Instance" and itais."Tag" = ''' + @ApprovazioneIntermediaSKULabel + '''
	where
		sku."ID" = 
		(
			--confirmed sku

			select max(sku2."ID")
			from 
				"CheckLists"."Instances" sku2
				inner join "CheckLists"."PrototypeVersions" pv2 on sku2."Prototype" = pv2."Prototype" and sku2."Version" = pv2."Version" and pv2."VersionState" in (''Active'', ''Obsolete'')
				inner join "CheckLists"."InstanceTags" itp2 on sku2."ID" = itp2."Instance" and itp2."Tag" = ''' + @ProdottoLabel + ''' and itp2."Value" = itp."Value"
				inner join "CheckLists"."InstanceTags" itl2 on sku2."ID" = itl2."Instance" and itl2."Tag" = ''' + @LineaLabel + ''' and itl2."Value" = itl."Value"
				left join "CheckLists"."InstanceTags" itais2 on sku2."ID" = itais2."Instance" and itais2."Tag" = ''' + @ApprovazioneIntermediaSKULabel + '''
			where
				sku2."PrototypeName" = ''' + @SKUPrototype + '''
				and sku2."InstanceState" = ''Confirmed''
		)
		or ''' + @OnlyConfirmed + ''' = ''N'' and sku."ID" = 
		(
			--intermediate sku

			select max(sku2."ID")
			from 
				"CheckLists"."Instances" sku2
				inner join "CheckLists"."PrototypeVersions" pv2 on sku2."Prototype" = pv2."Prototype" and sku2."Version" = pv2."Version" and pv2."VersionState" in (''Active'', ''Obsolete'')
				inner join "CheckLists"."InstanceTags" itp2 on sku2."ID" = itp2."Instance" and itp2."Tag" = ''' + @ProdottoLabel + ''' and itp2."Value" = itp."Value"
				inner join "CheckLists"."InstanceTags" itl2 on sku2."ID" = itl2."Instance" and itl2."Tag" = ''' + @LineaLabel + ''' and itl2."Value" = itl."Value"
				left join "CheckLists"."InstanceTags" itais2 on sku2."ID" = itais2."Instance" and itais2."Tag" = ''' + @ApprovazioneIntermediaSKULabel + '''
			where
				sku2."PrototypeName" = ''' + @SKUPrototype + '''
				and sku2."InstanceState" = ''In progress'' and itais2."Value" = ''1''
		)

'

set @Query = '
	select * from openquery(CLM_DEV, ''' + replace(@Query, '''', '''''') + ''')';
		
exec(@Query);
