# run tests
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\Parser.Tests
# print dotnet version that is used for running test
dotnet --info
# --no-build make sure we don't waste time in building again.
# --verbosity help us know which test passed.
# --logger is supposed to print console logs but it does not work.
dotnet test --no-build --diag testruns.log --verbosity normal --logger "console;verbosity=normal"
popd