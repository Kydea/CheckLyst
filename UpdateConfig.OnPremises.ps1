$connectionString = 'metadata=res://*/RelationalDatabaseModel.csdl|res://*/RelationalDatabaseModel.ssdl|res://*/RelationalDatabaseModel.msl;provider=Npgsql;provider connection string="Host=db-02.clm.internal;Database=CheckListManager;Username=CheckListManager;Password=CheckListManager01;Persist Security Info=True"'
$awsAccessKey = "-"
$awsSecretKey = "-"
$awsRegion = "-"
$queueSystem = "RabbitMQ"
$rabbitMQHost = "rabbitmq.clm.internal"
$rabbitMQUsername = "CheckListManager"
$rabbitMQPassword = "CheckListManager01"
$storageSystem = "FileSystem"
$system = "AbbVie"
$environment = "Local"


.\UpdateConfig.ps1 -ConnectionString $connectionString -AWSAccessKey $awsAccessKey -AWSSecretKey $awsSecretKey -AWSRegion $awsRegion -QueueSystem $queueSystem -RabbitMQHost $rabbitMQHost -RabbitMQUsername $rabbitMQUsername -RabbitMQPassword $rabbitMQPassword -StorageSystem $storageSystem -System $system -Environment $environment