
# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Service Fabric Backup Explorer ( Review and update utility for Service Fabric Reliable Collections)

The project empowers the Service Fabric Reliable Statefule Application users to audit and review the transactions performed on reliable collections and edit the current state of reliable collection to consistent view.
It also creates backup of the current snapshot of the Reliable Collections which can be loaded in any of the current Service Fabric Cluster which has the same implementation of Reliable Collections, and runnning the same implementation/version of the Reliable Stateful Application.

The changes made to current state will be restored along with the other transactions to current running Service Fabric Cluster, hence enabling a consistent view of the collection.

Service Fabric Backup Explorer helps in data correction in case of data corruption. The current state of data can be corrupted because of any bug introduced in application or any wrong entries made in the live clusters.

With the help of Backup Explorer following tasks can be performed :
 
* Querying of metadata for the collection.  
* Current state and its entries in the collection of the backup loaded.
* Enlist the transactions performed.
* Update the collection by adding, updating or deleting the entries in the collection. 
* Backup the data post update.
 
The Service Fabric Backup Explorer can be consumed in any of the following ways for view/update of reliable collections of the backup.

1. Binary -         Nuget package to view and alter the reliable collections.
2. HTTP/Rest   -    HTTP based  Rest hosting to view and alter the reliable collections.
3. bkpctl -         Service fabric backup controller CLI (command line interface) to view and alter the reliable collections. 

# Microsoft.ServiceFabric.ReliableCollectionBackup.Parser 
The binary dll created to be consumed in application to view, enumerate and alter the reliable collection.
Details [ Binary Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.nuproj ](docs/Microsoft.ServiceFabric.ReliableCollectionBackup.Parser)

# HTTP Rest 
The OWIN based rest API to view, enumerate and alter the state of reliable collection.
Details [ HTTP Rest ](docs/rest)

# bkpctl
The cli based interface to view, enumerate and alter the state of Reliable Collections.
Details [ bkpctl ](docs/bkpctl)


# Requirements
1. dotnet
2. msbuild
3. nuget
4. python
5. python-pip
6. Service Fabric Runtime


# Building RestServer and Parser Code 
```
 .\build_all.ps1 -build
```

# Build all packages including Tests and Backup Command Line Tool
From Repository root folder , run in Powershell(after adding MSBuild to the Path of Command Line):
```
 .\build_all.ps1 -buildAll
```

# Generating nupkg
From Visual Studio Command Prompt which has msbuild defined:
```
.\build_all.ps1 -build -generateNupkg
```

# Running Rest Server
From the Repository root folder, perform the following steps :
```
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer\
dotnet build
xcopy.exe /EIYS ..\..\..\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.5.659-beta\lib\netstandard2.0\*.dll bin\Debug\net471\
dotnet run --no-build --config configs\sampleconfig.json

# Testing REST API's
curl -v http://localhost:5000/$query/testDictionary?$top=2

popd
```

# Running Tests
```
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\Parser.Tests
dotnet build
dotnet test --no-build --diag test_results.log --verbosity n --logger "console;verbosity=detailed"
popd

# running one test
dotnet test --no-build --diag test_results.log --verbosity n --logger "console;verbosity=detailed" --filter "FullyQualifiedName~BackupParser_EachTransactionHasRightChangesEvenWithBlockingTransactionAppliedEvents"
```

Running RestServer tests:
```
cd service-fabric-backup-explorer\src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer.Tests
dotnet build && \
xcopy.exe /EIYS ..\..\..\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.5.659-beta\lib\netstandard2.0\*.dll bin\Debug\net471\ && \
dotnet test --no-build --diag test_results.log --verbosity normal --logger "console;verbosity=detailed"
```

# Build and Consume bkpctl
From Repository root folder , run in Powershell:
```
 .\build_all.ps1 -buildCli
```
In order to use bkpctl, the REST Server must be up and running, so that CLI can fetch the required backup, and present it on the command line . 


