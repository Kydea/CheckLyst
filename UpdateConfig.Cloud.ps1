$connectionString = "metadata=res://*/RelationalDatabaseModel.csdl|res://*/RelationalDatabaseModel.ssdl|res://*/RelationalDatabaseModel.msl;provider=Npgsql;provider connection string=&quot;Host=db-01.clm.internal;Database=CheckListManager;Username=CheckListManager;Password=CheckListManager01;Persist Security Info=True&quot;"
$awsAccessKey = "AKIAISETYTF5XAR3ZNSA"
$awsSecretKey = "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"
$awsRegion = "eu-west-1"
$queueSystem = "SQS"
$rabbitMQHost = "-"
$rabbitMQUsername = "-"
$rabbitMQPassword = "-"
$storageSystem = "S3"


.\UpdateConfig.ps1 -ConnectionString $connectionString -AWSAccessKey $awsAccessKey -AWSSecretKey $awsSecretKey -AWSRegion $awsRegion -QueueSystem $queueSystem -RabbitMQHost $rabbitMQHost -RabbitMQUsername $rabbitMQUsername -RabbitMQPassword $rabbitMQPassword -StorageSystem $storageSystem