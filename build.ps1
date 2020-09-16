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
    [switch]$nuget,

    # Do All the Above tasks together
    [switch]$buildAll,
    
    # Build the bkpctl Cli
    [switch]$buildCli,

    # Path to MSBuild if VS2017 is installed in non-conventional Location
    [string]$MSBuildFullPath,

    # Version of VS Installed
    [ValidateSet('2017','2019')]
    [string]$vsversion ="2017",

    # Build configuration - Default is Debug
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
);

# Include comman commands.
. "./common.ps1"

$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$NugetFullPath = join-path $PSScriptRoot "nuget.exe"


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
    throw "Unable to find MSBuild installed on this machine. Please install Visual Studio 2017 or if its installed at non-default location, provide the full path to msbuild using -MSBuildFullPath parameter."
}

$DisplayHelp = $true;

if ($showVersion) {
    $DisplayHelp = $false;
    Write-Host "Version of all tools:"
    dotnet --info
    Write-Host "MSBuild Version:"
    & $MSBuildFullPath  /version
}

if ($build -Or $buildAll) {
    $DisplayHelp = $false;
    # build all projects
    Write-Host "Building Backup Explorer Code and Tests:"
    Exec { dotnet build --packages .\packages backupExplorer.sln -c $Configuration }

    # publish for nupkg generation
    pushd src\BackupExplorer\Parser\
    Exec { dotnet publish --no-build --framework netstandard2.0 -c $Configuration }
    Exec { dotnet publish --no-build --framework net48 -c $Configuration }
    popd

    pushd src\BackupExplorer\RestServer\
    Exec { dotnet publish --no-build -c $Configuration }
    popd
    
    xcopy .\src\backup-explorer-cli\* .\bin\backupCLI /E  /C /I /Y

}

if ($nuget -Or $buildAll) {
    $DisplayHelp = $false;
    Write-Host "Generating Nuget Package:"

    pushd nuprojs
    # nuget restore
    Exec { & $NugetFullPath restore -Verbosity detailed .nuget\packages.config -PackagesDirectory .\packages }
    # generate nupkg
    Exec { & $MSBuildFullPath Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.nuproj /p:OutputPath=..\bin\  /p:Configuration=$Configuration}
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
    Write-Host "3. -nuget Generate the nuget package"
    Write-Host "4. -showVersion Displays versions of all tools"
    Write-Host "5. -buildCli Builds the bkpctl CLI tool for viewing and editing Backups"
    Write-Host "6. -vsversion  User can specify the version of Visual Studio to use , VS 2017 by default."
    Write-Host "7. -MSBuildFullPath User can specify where to look for MSBuild in case VS is installed in unusual Location"
    Write-Host "8. -Configuration User can specify either to build in Debug or Release "
}