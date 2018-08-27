# build all projects
dotnet build service-fabric-backup-explorer.sln

# publish for nupkg generation
pushd RCBackupParser
dotnet publish
popd

# nuget restore
nuget restore service-fabric-backup-explorer.sln

# generate nupkg
pushd nuprojs
msbuild Microsoft.ServiceFabric.Tools.RCBackupParser.nuproj
popd
