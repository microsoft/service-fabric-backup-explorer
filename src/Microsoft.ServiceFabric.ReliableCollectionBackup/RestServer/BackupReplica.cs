using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    class BackupReplica
    {
        public async Task<IWebHostBuilder> CreateWebHostBuilder(BackupChainInfo backupInfo)
        {
            var backupParser = await BringupBackupParser(backupInfo.BackupChainPath, backupInfo.CodePackagePath);

            // todo : set root host to AppName/ServiceName
            return WebHost.CreateDefaultBuilder()
                .ConfigureServices(
                    services => services
                                .AddSingleton<StatefulServiceContext>(backupParser.GetStatefulServiceContext())
                                .AddSingleton<IReliableStateManager>(backupParser.StateManager)
                                .AddSingleton<BackupParser>(backupParser))
                .UseStartup<Startup>();
        }

        async Task<BackupParser> BringupBackupParser(string backupChainPath, string codePackagePath)
        {
            var backupParser = new BackupParser(backupChainPath, codePackagePath);
            await backupParser.ParseAsync(CancellationToken.None); // todo: take in configuration
            return backupParser;
        }
    }
}
