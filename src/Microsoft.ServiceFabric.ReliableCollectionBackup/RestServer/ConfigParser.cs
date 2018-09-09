using System;
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

        public IEnumerable<BackupChainInfo> GetBackupChainInfo()
        {
            return this.backupChainInfos;
        }

        void Parse(Stream jsonStream)
        {
            StreamReader reader = new StreamReader(jsonStream);
            string jsonContent = reader.ReadToEnd();

            var config = JObject.Parse(jsonContent);
            JToken backupChainTokens = null;
            if (config.TryGetValue(BackupChainInfosKey, System.StringComparison.OrdinalIgnoreCase, out backupChainTokens))
            {
                foreach(var backupChainToken in backupChainTokens)
                {
                    backupChainInfos.Add(backupChainToken.ToObject<BackupChainInfo>());
                }
            }

            this.Validate();
        }

        void Validate()
        {
            if (this.backupChainInfos.Count == 0)
            {
                throw new InvalidDataException(
                    string.Format("BackupChainInfos is a required field in config : {0}", this.configPath));
            }

            foreach (var backupInfo in backupChainInfos)
            {
                backupInfo.ValidateAndSetDefaultValues();
            }
        }

        string configPath;
        IList<BackupChainInfo> backupChainInfos = new List<BackupChainInfo>();
        const string BackupChainInfosKey = "BackupChainInfos";
    }
}
