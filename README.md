
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


# Building code
```
powershell .\build_all.ps1 -build
```

# Generating nupkg
From Visual Studio command prompt which has msbuild defined
```
powershell .\build_all.ps1 -build -generateNupkg
```

# Running Rest Server
```
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer\
dotnet build
xcopy.exe /EIYS ..\..\..\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.4.187-beta\lib\netstandard2.0\*.dll bin\Debug\net471\
dotnet run --no-build ..\Parser.Tests\UserFullBackup ..\Parser.Tests\UserType\bin
popd
```