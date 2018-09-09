using System;
using System.Fabric;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    internal class BackupWebHost
    {
        public BackupWebHost(BackupChainInfo backupChainInfo)
        {
            this.backupChainInfo = backupChainInfo;
        }

        async public Task Run()
        {
            var webHost = await this.CreateWebHostBuilder();
            webHost.Build().Run();
        }

        async Task<IWebHostBuilder> CreateWebHostBuilder()
        {
            var backupParser = await BringupBackupParser();

            // todo : set root host to AppName/ServiceName
            return WebHost.CreateDefaultBuilder()
                .ConfigureServices(
                    services => services
                                .AddSingleton<StatefulServiceContext>(backupParser.GetStatefulServiceContext())
                                .AddSingleton<IReliableStateManager>(backupParser.StateManager)
                                .AddSingleton<BackupParser>(backupParser))
                .UseStartup<Startup>();
        }

        async Task<BackupParser> BringupBackupParser()
        {
            var backupParser = new BackupParser(this.backupChainInfo.BackupChainPath, this.backupChainInfo.CodePackagePath);
            var stateManager = backupParser.StateManager;

            foreach (var serializer in this.backupChainInfo.Serializers)
            {
                var stateTypeName = serializer.StateFullyQualifiedTypeName;
                var serializerTypeName = serializer.SerializerFullyQualifiedTypeName;

                var stateType = Type.GetType(stateTypeName, true);
                var serializerType = Type.GetType(serializerTypeName, true);
                var serializerObject = Activator.CreateInstance(serializerType);

                stateManager.GetType()
                    .GetMethod("TryAddStateSerializer", BindingFlags.Instance | BindingFlags.Public)
                    .MakeGenericMethod(stateType)
                    .Invoke(stateManager, new object[] { serializerObject });
            }

            await backupParser.ParseAsync(CancellationToken.None); // todo: take in configuration
            return backupParser;
        }

        private BackupChainInfo backupChainInfo;
    }
}
