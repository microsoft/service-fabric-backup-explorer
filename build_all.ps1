##
#  Builds the source code and generates nuget packages. You can optionally just build the source code by opening individual solutions in Visual Studio.
##

param
(
    # show versions of all tools used
    [switch]$showVersion

    # build code
    [switch]$build

    # generate nupkg
    [switch]$generateNupkg
)

if ($showVersion) {
    Write-Host "Version of all tools:"
    dotnet --info
    msbuild --version
    nuget
}

if ($build) {
    # build all projects
    Write-Host "Building code and tests:"
    dotnet build service-fabric-backup-explorer.sln
}

if ($generateNupkg) {
    Write-Host "Generating nupkg:"

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
}
