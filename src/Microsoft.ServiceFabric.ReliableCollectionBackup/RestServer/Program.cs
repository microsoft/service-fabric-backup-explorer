// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;

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

            var process = Process.GetCurrentProcess();
            Console.WriteLine("Process Name/Id of RestServer : {0}/{1}", process.ProcessName, process.Id);

            Task.Run(async () => {
                var webHostBuilder = await CreateWebHostBuilder(args);
                webHostBuilder.Build().Run();
            }).Wait();
        }

        public static async Task<IWebHostBuilder> CreateWebHostBuilder(string[] args)
        {
            var backupParser = await BringupBackupParser(args.Take(2).ToArray());

            return WebHost.CreateDefaultBuilder(args.Skip(2).ToArray())
                .ConfigureServices(
                    services => services
                                .AddSingleton<StatefulServiceContext>(backupParser.GetStatefulServiceContext())
                                .AddSingleton<IReliableStateManager>(backupParser.StateManager)
                                .AddSingleton<BackupParser>(backupParser))
                .UseStartup<Startup>();
        }

        static void PrintUsage()
        {
            Console.WriteLine("Start the server with 2 arguments: ");
            Console.WriteLine("1. Location of backup folder");
            Console.WriteLine("2. Location of code packages");
        }

        static async Task<BackupParser> BringupBackupParser(string[] args)
        {
            var backupDirectory = args[0];
            var codeDirectory = args[1];
            var backupParser = new BackupParser(backupDirectory, codeDirectory);
            await backupParser.ParseAsync(CancellationToken.None);
            return backupParser;
        }
    }
}
