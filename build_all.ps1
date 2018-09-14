##
#  Builds the source code and generates nuget packages. You can optionally just build the source code by opening individual solutions in Visual Studio.
##

param
(
    # show versions of all tools used
    [switch]$ToolVersion,

    # generate nupkg
    [switch]$GenerateNuget,

    # nuget source
    [string]$NugetSource = "https://api.nuget.org/v3/index.json"
);

# Include comman commands.
. "./common.ps1"

if ($ToolVersion) {
	Write-Host "Version of all tools:"
	dotnet --info
	msbuild /version
	exit
}

#restore the packages needed at .\packages
Write-Host "Restoring packages" -ForegroundColor Cyan
Exec { dotnet restore --packages .\packages service-fabric-backup-explorer.sln -s $NugetSource }

# build all projects
Write-Host "Building code and tests:" -ForegroundColor Cyan
Exec { dotnet build --packages .\packages service-fabric-backup-explorer.sln }

# publish for nupkg generation
Exec { dotnet publish --no-build }

# Rest Server: copy our dlls in publish folder
Exec { xcopy.exe /EIYS .\packages\microsoft.servicefabric.tools.reliabilitysimulator\6.4.187-beta\lib\netstandard2.0\*.dll .\bin\publish\Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer\ }

if ($GenerateNuget) {
    Write-Host "Generating nuget:" -ForegroundColor Cyan

    pushd nuprojs
    # nuget restore
    Exec { K:\rdnext\service-fabric-backup-explorer\packages\system.linq.dynamic\1.0.7\NuGet.exe restore -Verbosity detailed .nuget\packages.config -PackagesDirectory .\packages }
    # generate nupkg
    Exec { msbuild Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.nuproj /p:OutputPath=$PSScriptRoot\bin\nupkg }
    popd
}

Write-Host "Done."