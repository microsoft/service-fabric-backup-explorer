##
#  Builds the source code and generates nuget packages. You can optionally just build the source code by opening individual solutions in Visual Studio.
##

param
(
    # show versions of all tools used
    [switch]$showVersion,

    # build code
    [switch]$build,

    # generate nupkg
    [switch]$generateNupkg
);

if ($showVersion) {
    Write-Host "Version of all tools:"
    dotnet --info
    msbuild /version
    nuget
}

if ($build) {
    # build all projects
    Write-Host "Building code and tests:"
    dotnet build --packages .\packages service-fabric-backup-explorer.sln

    # publish for nupkg generation
    dotnet publish --no-build

    # Rest Server: copy our dlls in publish folder
    xcopy.exe /EIYS .\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.4.186-beta\lib\netstandard2.0\*.dll .\bin\publish\Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer\
}

if ($generateNupkg) {
    Write-Host "Generating nupkg:"

    pushd nuprojs
    # nuget restore
    nuget.exe restore -Verbosity detailed .nuget\packages.config -PackagesDirectory .\packages
    # generate nupkg
    msbuild Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.nuproj /p:OutputPath=$PSScriptRoot\bin\nupkg
    popd
}
