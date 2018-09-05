# Keep all changes
git add -A .
# Clean up
git clean -xdf

# build and run tests
./build_all.ps1 -build
./run_unittests.ps1

# verify rest server
pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer
xcopy.exe /EIYS ..\..\..\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.4.187-beta\lib\netstandard2.0\*.dll bin\Debug\net471\
# I am not sure how to run this in background
dotnet run --no-build ..\Parser.Tests\UserFullBackup ..\Parser.Tests\UserType\bin
curl -v http://localhost:5000/api/exit
popd