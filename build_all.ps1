##
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
##

##
#  Builds the source code and generates nuget packages. You can optionally just build the source code by opening individual solutions in Visual Studio.
##

param
(
    # Show versions of all tools used
    [switch]$showVersion,

    # Build Code
    [switch]$build,

    # Generate NuPkg
    [switch]$generateNupkg,

    # Do All the Above tasks together
    [switch]$buildAll,
    
    # Build the bkpctl Cli
    [switch]$buildCli,

    # Path to MSBuild if VS2017 is installed in non-conventional Location
    [string]$MSBuildFullPath,

    # Version of VS Installed
    [ValidateSet('2017','2019')]
    [string]$vsversion ="2017"
);

# Include comman commands.
. "./common.ps1"


# msbuild path not provided, find msbuild for VS2017
if ($MSBuildFullPath -eq "") {
    if (${env:VisualStudioVersion} -eq "15.0" -and ${env:VSINSTALLDIR} -ne "") {
        $MSBuildFullPath = join-path ${env:VSINSTALLDIR} "MSBuild\15.0\Bin\MSBuild.exe"
    }
}

if ($MSBuildFullPath -eq "") {
    if (Test-Path "env:\ProgramFiles(x86)") {
        $progFilesPath = ${env:ProgramFiles(x86)}
    }
    elseif (Test-Path "env:\ProgramFiles") {
        $progFilesPath = ${env:ProgramFiles}
    }

    $VS2017InstallPath = join-path $progFilesPath "Microsoft Visual Studio\${vsversion}"
    $versions = 'Community', 'Professional', 'Enterprise'

    $versionno=''
    foreach ($version in $versions) {
        if ($vsversion -eq "2019") {
            $versionno = '\Current'
        }
        else {
            $versionno = '15.0'
        }
        $VS2017VersionPath = join-path $VS2017InstallPath $version
        $MSBuildFullPath = join-path $VS2017VersionPath "MSBuild\${versionno}\Bin\MSBuild.exe"

        if (Test-Path $MSBuildFullPath) {
           break
        }
    }    
}

if (!(Test-Path $MSBuildFullPath)) {
    throw "Unable to find MSBuild installed on this machine. Please install Visual Studio 2017 or if its installed at non-default location, provide the full ppath to msbuild using -MSBuildFullPath parameter."
}

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
    Write-Host "Building Backup Explorer Code and Tests:"
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
    Exec { xcopy.exe /EIYS .\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.5.659-beta\lib\netstandard2.0\*.dll .\src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer\bin\Debug\net471\ }
}

if ($generateNupkg -Or $buildAll) {
    $DisplayHelp = $false;
    Write-Host "Generating nupkg:"

    pushd nuprojs
    # nuget restore
    Exec { nuget.exe restore -Verbosity detailed .nuget\packages.config -PackagesDirectory .\packages }
    # generate nupkg
    Exec { & $MSBuildFullPath Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.nuproj /p:OutputPath=..\bin\nupkg }
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
    Write-Host "5. -buildCli Builds the bkpctl CLI tool for viewing and editing Backups"
}