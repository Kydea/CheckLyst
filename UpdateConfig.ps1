param (
	[parameter(Mandatory=$true)]
	[String]$ConnectionString,
	[parameter(Mandatory=$true)]
	[String]$AWSAccessKey,
	[parameter(Mandatory=$true)]
	[String]$AWSSecretKey,
	[parameter(Mandatory=$true)]
	[String]$AWSRegion,
	[parameter(Mandatory=$true)]
	[String]$QueueSystem,
	[parameter(Mandatory=$true)]
	[String]$RabbitMQHost,
	[parameter(Mandatory=$true)]
	[String]$RabbitMQUsername,
	[parameter(Mandatory=$true)]
	[String]$RabbitMQPassword,
	[parameter(Mandatory=$true)]
	[String]$StorageSystem
)

$files = dir "$PSScriptRoot\*" -include ('App.config', 'Web.Config', 'ConnectionStrings.Config') -recurse

foreach ($file in $files) {
    [xml]$config = (Get-Content -Path $file.FullName)

    if ($config.connectionStrings) {
        $config.connectionStrings.add.connectionString = $ConnectionString
    }
    
    $setting = $config.SelectSingleNode("/configuration/appSettings/add[@key='AWSAccessKey']")    
    if ($setting) {
        $setting.value = $AWSAccessKey
    }
    $setting = $config.SelectSingleNode("/configuration/appSettings/add[@key='AWSSecretKey']")    
    if ($setting) {
        $setting.value = $AWSSecretKey
    }
    $setting = $config.SelectSingleNode("/configuration/appSettings/add[@key='AWSRegion']")    
    if ($setting) {
        $setting.value = $AWSRegion
    }
    $setting = $config.SelectSingleNode("/configuration/appSettings/add[@key='QueueSystem']")    
    if ($setting) {
        $setting.value = $QueueSystem
    }
    $setting = $config.SelectSingleNode("/configuration/appSettings/add[@key='RabbitMQHost']")    
    if ($setting) {
        $setting.value = $RabbitMQHost
    }
    $setting = $config.SelectSingleNode("/configuration/appSettings/add[@key='RabbitMQUsername']")    
    if ($setting) {
        $setting.value = $RabbitMQUsername
    }
    $setting = $config.SelectSingleNode("/configuration/appSettings/add[@key='RabbitMQPassword']")    
    if ($setting) {
        $setting.value = $RabbitMQPassword
    }
    $setting = $config.SelectSingleNode("/configuration/appSettings/add[@key='StorageSystem']")
    if ($setting) {
        $setting.value = $StorageSystem
    }

    $config.Save($file.FullName)
}