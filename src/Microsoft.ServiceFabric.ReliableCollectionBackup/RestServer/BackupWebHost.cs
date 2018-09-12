using System;
using System.Collections.Generic;
using System.Fabric;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    /// <summary>
    /// Http webhost wrapper class for a BackupChain rest server
    /// </summary>
    internal class BackupWebHost
    {
        /// <summary>
        /// Constructor of BackupWebHost
        /// </summary>
        /// <param name="backupChainInfo">BackupChainInfo for which to run web host</param>
        public BackupWebHost(BackupChainInfo backupChainInfo)
        {
            this.backupChainInfo = backupChainInfo;
        }

        /// <summary>
        /// Starts web host.
        /// </summary>
        /// <returns>Task representing asynchronous web host run operation.</returns>
        async public Task Run()
        {
            await Task.Run(() =>
            {
                var webHost = this.CreateWebHostBuilder();
                webHost.Build().Run();
            });
        }

        private IWebHostBuilder CreateWebHostBuilder()
        {
            var backupParserManager = this.SetupBackupParserManagerAndStartParsing();
            var appBasePath = string.Format("/{0}/{1}", this.backupChainInfo.AppName, this.backupChainInfo.ServiceName);
            var config = this.BuildConfig(appBasePath);

            return WebHost.CreateDefaultBuilder()
                .UseConfiguration(config)
                .ConfigureServices(
                    services => services
                                // Need StatefulServiceContext and IReliableStateManager for queryable middleware.
                                .AddSingleton<StatefulServiceContext>(backupParserManager.BackupParser.GetStatefulServiceContext())
                                .AddSingleton<IReliableStateManager>(backupParserManager.BackupParser.StateManager)
                                .AddSingleton<BackupParserManager>(backupParserManager))
                .UseStartup<Startup>();
        }

        private BackupParserManager SetupBackupParserManagerAndStartParsing()
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

            var backupParserManager = new BackupParserManager(backupParser);
            backupParserManager.StartParsing();
            return backupParserManager;
        }

        private IConfiguration BuildConfig(string appBasePath)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "AppBasePath", appBasePath } })
                .Build();
        }

        private BackupChainInfo backupChainInfo;
    }
}
