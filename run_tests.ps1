# Include comman commands.
. "./common.ps1"

# run tests
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\Parser.Tests
# print dotnet version that is used for running test
Exec { dotnet --info }
# --no-build make sure we don't waste time in building again.
# --verbosity help us know which test passed.
# --logger is supposed to print console logs but it does not work.
Exec { dotnet test --no-build --diag test_results.log --verbosity normal --logger "console;verbosity=detailed" }
popd

# run rest server tests
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer.Tests
Exec { xcopy.exe /EIYS ..\..\..\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.4.187-beta\lib\netstandard2.0\*.dll bin\Debug\net471\ }
Exec { dotnet test --no-build --diag test_results.log --verbosity normal --logger "console;verbosity=detailed" }
popd