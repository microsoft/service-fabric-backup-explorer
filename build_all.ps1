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
    [switch]$generateNupkg,

    # generate nupkg
    [switch]$buildAll,
    
    # generate nupkg
    [switch]$buildCli
);

# Include comman commands.
. "./common.ps1"

$DisplayHelp = $true;

if ($showVersion) {
    $DisplayHelp = $false;
    Write-Host "Version of all tools:"
    dotnet --info
    msbuild /version
    nuget
}

if ($build -Or $buildAll) {
    $DisplayHelp = $false;
    # build all projects
    Write-Host "Building code and tests:"
    Exec { dotnet build --packages .\packages service-fabric-backup-explorer.sln }

    # publish for nupkg generation
    pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\Parser\
    Exec { dotnet publish --no-build --framework netstandard2.0 -c "Debug" }
    Exec { dotnet publish --no-build --framework net461 -c "Debug" }
    popd

    pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer\
    Exec { dotnet publish --no-build -c "Debug" }
    popd

    # Rest Server: copy our dlls in publish folder
    Exec { xcopy.exe /EIYS .\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.5.659-beta\lib\netstandard2.0\*.dll .\bin\publish\Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer\ }
}

if ($generateNupkg -Or $buildAll) {
    $DisplayHelp = $false;
    Write-Host "Generating nupkg:"

    pushd nuprojs
    # nuget restore
    Exec { nuget.exe restore -Verbosity detailed .nuget\packages.config -PackagesDirectory .\packages }
    # generate nupkg
    Exec { msbuild Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.nuproj /p:OutputPath=..\bin\nupkg }
    popd
}

if ($buildCli -Or $buildAll) {
    $DisplayHelp = $false;
    Write-Host "Building Service Fabric Backup Explorer CLI bkpctl:"

    pushd src 
    #  Build bkpctl package
    Exec { pip install -e backup-explorer-cli }
    popd
}

if ($DisplayHelp) {
    Write-Host "Following options are available :"
    Write-Host "1. -build Builds the Code"
    Write-Host "2. -buildAll Builds the Code and generate Nuget package"
    Write-Host "3. -generateNupkg Generate the nuget package"
    Write-Host "4. -showVersion Displays versions of all tools"
}