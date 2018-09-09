using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    internal class ConfigParser
    {
        public ConfigParser(string configPath)
        {
            using (var stream = File.OpenRead(configPath))
            {
                this.Parse(stream);
            }
        }

        public ConfigParser(Stream configStream)
        {
            this.Parse(configStream);
        }
        public Configuration GetConfigration()
        {
            return this.configuration;
        }

        void Parse(Stream jsonStream)
        {
            StreamReader reader = new StreamReader(jsonStream);
            string jsonContent = reader.ReadToEnd();

            var config = JObject.Parse(jsonContent);
            JToken backupChainTokens = null;
            var backupChainInfos = new List<BackupChainInfo>();

            if (config.TryGetValue(BackupChainInfosKey, System.StringComparison.OrdinalIgnoreCase, out backupChainTokens))
            {
                foreach(var backupChainToken in backupChainTokens)
                {
                    backupChainInfos.Add(backupChainToken.ToObject<BackupChainInfo>());
                }
            }

            this.Validate(backupChainInfos);
            this.configuration = new Configuration(backupChainInfos);
        }

        void Validate(IList<BackupChainInfo> backupChainInfos)
        {
            if (backupChainInfos.Count == 0)
            {
                throw new InvalidDataException("BackupChainInfos is a required field in config");
            }

            foreach (var backupInfo in backupChainInfos)
            {
                backupInfo.ValidateAndSetDefaultValues();
            }
        }

        Configuration configuration;
        const string BackupChainInfosKey = "BackupChainInfos";
    }
}
