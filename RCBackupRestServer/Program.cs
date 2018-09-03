using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data;

namespace Microsoft.ServiceFabric.Tools.RCBackupRestServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // e:\service-fabric-backup-explorer\RCBackupParserTests\UserFullBackup
            // e:\service-fabric-backup-explorer\RCBackupParserTests\UserType\bin
            if (args.Length < 2)
            {
                PrintUsage();
                Environment.Exit(1);
            }

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
                                .AddSingleton<RCBackupParser.RCBackupParser>(backupParser))
                .UseStartup<Startup>();
        }

        static void PrintUsage()
        {
            Console.WriteLine("Start the server with 2 arguments: ");
            Console.WriteLine("1. Location of backup folder");
            Console.WriteLine("2. Location of code packages");
        }

        static async Task<RCBackupParser.RCBackupParser> BringupBackupParser(string[] args)
        {
            var backupDirectory = args[0];
            var codeDirectory = args[1];
            var backupParser = new RCBackupParser.RCBackupParser(backupDirectory, codeDirectory);
            await backupParser.ParseAsync(CancellationToken.None);
            return backupParser;
        }
    }
}
