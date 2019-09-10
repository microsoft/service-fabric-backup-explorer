// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                Environment.Exit(1);
            }

            if (args.Length > 2 && args[2].ToLower() == "-d")
            {
                Debugger.Launch();
            }

            var process = Process.GetCurrentProcess();
            Console.WriteLine("Process Name/Id of RestServer : {0}/{1}", process.ProcessName, process.Id);

            var configPath = args[1];
            var configuration = new ConfigParser(configPath).GetConfigration();

            var replicaTasks = new List<Task>();
            foreach (var backupInfo in configuration.GetBackupChainInfos())
            {
                replicaTasks.Add(StartBackupReplica(backupInfo));
            }

            Task.WaitAll(replicaTasks.ToArray());
        }

        static async Task StartBackupReplica(BackupChainInfo backupInfo)
        {
            var webHost = new BackupWebHost(backupInfo);
            await webHost.Run();
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer --config <path-to-config-file>");
        }
    }
}
