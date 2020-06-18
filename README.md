---
services: service-fabric
platforms: .NET, windows
owner: saketharsh, raunakpandya
---


# Service Fabric Backup Explorer (Preview)

[![Build Status](https://dev.azure.com/ms/service-fabric-backup-explorer/_apis/build/status/microsoft.service-fabric-backup-explorer?branchName=master)](https://dev.azure.com/ms/service-fabric-backup-explorer/_build/latest?definitionId=330&branchName=master)


### Review and Update Utility for Service Fabric Reliable Collections

The project empowers the Service Fabric Reliable Stateful application users to audit and review the contents of the Reliable Collections and edit the current state to a consistent view.
It also creates backup of the current snapshot of the Reliable Collections which can be loaded in any of the exisitng Service Fabric cluster which is running the same implementation/version of the Reliable Stateful application.

The changes made to current state will be restored along with the other transactions to current running Service Fabric cluster, hence enabling a consistent view of the collection.

Service Fabric Backup Explorer helps in data correction in case of data corruption. The current state of data can be corrupted because of any bug introduced in application or any wrong entries made in the live clusters.

With the help of Backup Explorer following tasks can be performed :
 
* Querying of metadata for the collection.  
* Current state and its entries in the collection of the backup loaded.
* Enlist the transactions performed (since the last checkpoint).
* Update the collection by adding, updating or deleting the entries in the collection. 
* Take a fresh backup with the udpated state.
 
The Service Fabric Backup Explorer can be consumed in any of the following ways for view/update of reliable collections of the backup.

1. Binary -         Nuget package to view and alter the reliable collections.
2. HTTP/Rest   -    HTTP based  Rest hosting to view and alter the reliable collections.
3. bkpctl -         Service fabric backup controller CLI (command line interface) to view and alter the reliable collections. 

## Microsoft.ServiceFabric.ReliableCollectionBackup.Parser 
The binary dll created to be consumed in application to view, enumerate and alter the reliable collection.

Details- [ Parser](docs/Parser)

## HTTP Rest Server
An OWIN based REST API to view, enumerate and alter the state of Reliable Collections.

**Currently, only the Serializers which do not take any parameters as input are supported in Rest Server. Users with serializers having multiple params should use the Nuget in order to perfrom CRUD operations in the Backup**

Details- [ RestServer ](docs/Server)

## bkpctl
A Command Line  Interface to view, enumerate and alter the state of Reliable Collections.

Details- [ bkpctl ](docs/bkpctl)


## Requirements
1. dotnet
2. MSBuild
3. Nuget
4. Python 3
5. python-pip

## Usage 
Download the latest release from the Repository.

### Installing bkpctl CLI 
To install the the CLI tool for easy I/O operations on the backup :-
```
cd %Release-Directory%
pip install backup-explorer-cli
```

### Using Rest Server 
```
cd %Release-Directory
cd bin\publish\Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
.\Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.exe --config %Your-Config-File.json%
```

### Using Parser Nuget 
Microsoft.ServiceFabric.ReliableCollectionBackup.Parser NuGet Package can be downloaded from Nuget.org official repository .
Details of Using the Nuget provided are mentioned in the [Parser docs](docs/Parser)

## Developer Help and & Documentation

### Building Rest Server and Parser Code 
From Repository root folder, run in Powershell:
```
 .\build_all.ps1 -build
```

### Build and Consume bkpctl
From Repository root folder , run in Powershell:
```
 .\build_all.ps1 -buildCli
```
In order to use bkpctl, the REST Server must be up and running, so that CLI can fetch the required backup, and present it on the command line . 


### Build all packages including Tests and Backup Command Line Tool
From Repository root folder , run in Powershell:
```
 .\build_all.ps1 -buildAll
```
User can choose to specify the path of MSBuild or the Visual Studio Version installed in the system. Default Version for VS is 2017.

### Generating Nuget Packages
From Visual Studio Command Prompt which has MSBuild defined:
```
.\build_all.ps1 -build -generateNupkg
```

### Running Rest Server
From the Repository root folder, perform the following steps :
```
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer\
dotnet build
dotnet run --no-build --config configs\sampleconfig.json --configuration Release

# Testing REST API's
curl -v http://localhost:5000/$query/testDictionary?$top=2

popd
```
### Configuring the Rest Server of Backup Viewer 
The Rest Server takes in as input, path to config json file, where user can specify the necessary configuratins for the Backup Viewer to Read the Backups. 

Sample Config file can be seen at [ sampleconfig.json](src/Microsoft.ServiceFabric.ReliableCollectionBackup/RestServer/configs/sampleconfig.json)

In the config file, user can define the following : 
1. App Name and Service Name 
2. BackupChainPath  - Location to where the Backups are located in the System 
3. CodePackagePath - Location to where binaries for Serializers and objects are located in the System. 
4. Serializers - It's an array, where user needs to specify all the Classes, and their Serializers that are required for the Backup Viewer to read the Backups 

### Running Tests
```
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\Parser.Tests
dotnet build
dotnet test --no-build --diag test_results.log --verbosity n --logger "console;verbosity=detailed" --configuration Release
popd

# running one test
dotnet test --no-build --diag test_results.log --verbosity n --logger "console;verbosity=detailed" --filter "FullyQualifiedName~BackupParser_EachTransactionHasRightChangesEvenWithBlockingTransactionAppliedEvents" --configuration Release
```

Running RestServer tests:
```
cd service-fabric-backup-explorer\src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer.Tests
dotnet build && \
dotnet test --no-build --diag test_results.log --verbosity normal --logger "console;verbosity=detailed"
```
## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Ideas and Improvements

We encourage community feedback and contribution to improve this application. To make any contribution, our contribution guidelines needs to be followed. If you have any new idea, please file an issue for that.

### Contribution Guidelines:
Please create a branch and push your changes to that and then, create a pull request for that change.
These is the check list that would be required to complete, for pushing your change to master branch.

1. Build the application with your change.
2. The application should satisfy all the test cases written for Parser.
3. Verify whether Rest Server is working correctly or not , using the Tests present in RestServer.Tests .
