# Include comman commands.
. "./common.ps1"

# run tests
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\Parser.Tests
# print dotnet version that is used for running test
Exec { dotnet --info }
# --no-build make sure we don't waste time in building again.
# --verbosity help us know which test passed.
# --logger is supposed to print console logs but it does not work.
Exec { dotnet test --no-build --diag test_results.log --verbosity normal --logger "console;verbosity=detailed" --configuration Release }
popd

# Uncomment these lines to run Rest Server Tests locally. 
#  pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer
#  Exec { dotnet run --no-build --config configs\sampleconfig.json   }
#  popd
#  pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer.Tests
#  Exec { dotnet test --no-build --diag test_results.log --verbosity normal --logger "console;verbosity=detailed" }
#  popd