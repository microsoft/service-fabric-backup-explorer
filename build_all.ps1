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

# Include comman commands.
. "./common.ps1"

if ($showVersion) {
    Write-Host "Version of all tools:"
    dotnet --info
    msbuild /version
    nuget
}

if ($build) {
    # build all projects
    Write-Host "Building code and tests:"
    Exec { dotnet build --packages .\packages service-fabric-backup-explorer.sln }

    # publish for nupkg generation
    pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\Parser\
    Exec { dotnet publish --no-build --framework netstandard2.0 -c "x64\Debug" }
    Exec { dotnet publish --no-build --framework net461 -c "x64\Debug" }
    popd

    pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer\
    Exec { dotnet publish }
    popd

    # Rest Server: copy our dlls in publish folder
    Exec { xcopy.exe /EIYS .\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.4.187-beta\lib\netstandard2.0\*.dll .\bin\publish\Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer\ }
}

if ($generateNupkg) {
    Write-Host "Generating nupkg:"

    pushd nuprojs
    # nuget restore
    Exec { nuget.exe restore -Verbosity detailed .nuget\packages.config -PackagesDirectory .\packages }
    # generate nupkg
    Exec { msbuild Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.nuproj /p:OutputPath=$PSScriptRoot\bin\nupkg }
    popd
}

Write-Host "Done."