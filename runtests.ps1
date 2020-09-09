##
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
##

##
#  Runs Unit Tests on the genrated builds to verify if all existing functionalites are working correctly
##

param
(
    # Build configuration - Default is Debug
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
);


# Include comman commands.
. "./common.ps1"

# run tests
pushd src\BackupExplorer\Parser.Tests
# print dotnet version that is used for running test
Exec { dotnet --info }
# --no-build make sure we don't waste time in building again.
# --verbosity help us know which test passed.
# --logger is supposed to print console logs but it does not work.
Exec { dotnet test --no-build --diag test_results.log --verbosity normal --logger "console;verbosity=detailed" --configuration $Configuration }
popd

# Uncomment these lines to run Rest Server Tests locally. 
#  pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer
#  Exec { dotnet run --no-build --config configs\sampleconfig.json   }
#  popd
#  pushd src\Microsoft.ServiceFabric.ReliableCollectionBackup\RestServer.Tests
#  Exec { dotnet test --no-build --diag test_results.log --verbosity normal --logger "console;verbosity=detailed" }
#  popd