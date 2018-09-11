using System;
using System.Collections.Generic;
using System.Fabric;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
            await Task.Run(() =>
            {
                var webHost = this.CreateWebHostBuilder();
                webHost.Build().Run();
            });
        }

        IWebHostBuilder CreateWebHostBuilder()
        {
            var backupParserWrapper = this.BringupBackupParser();
            var appBasePath = string.Format("/{0}/{1}", this.backupChainInfo.AppName, this.backupChainInfo.ServiceName);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { {"AppBasePath", appBasePath} })
                .Build();

            return WebHost.CreateDefaultBuilder()
                .UseConfiguration(config)
                .ConfigureServices(
                    services => services
                                .AddSingleton<StatefulServiceContext>(backupParserWrapper.BackupParser.GetStatefulServiceContext())
                                .AddSingleton<IReliableStateManager>(backupParserWrapper.BackupParser.StateManager)
                                .AddSingleton<BackupParser>(backupParserWrapper.BackupParser)  // remove this
                                .AddSingleton<BackupParserWrapper>(backupParserWrapper))
                .UseStartup<Startup>();
        }

        BackupParserWrapper BringupBackupParser()
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

            var backupParserWrapper = new BackupParserWrapper(backupParser);
            backupParserWrapper.StartParsing();
            return backupParserWrapper;
        }

        private BackupChainInfo backupChainInfo;
    }
}
