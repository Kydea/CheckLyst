build command da prompt dei comandi per sviluppatori di visual studio 2017
cd %path%/CheckLyst/Development/MigrationTools
msbuild /p:DeployOnBuild=true /p:DeployIisAppPath="./MigrationTools.sln" /p:PackageFileName="./publish/Package.zip"