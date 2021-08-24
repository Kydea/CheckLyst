build command: 
cd %path%/CheckLyst/Development/MigrationTools
msbuild /p:DeployOnBuild=true /p:DeployIisAppPath="./MigrationTools.sln" /p:PackageFileName="./publish/Package.zip"