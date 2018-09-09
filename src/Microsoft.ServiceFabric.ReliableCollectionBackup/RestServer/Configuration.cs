using System.Collections.Generic;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    internal class Configuration
    {
        public Configuration(IList<BackupChainInfo> backupChainInfos)
        {
            this.backupChainInfos = backupChainInfos;
        }

        public IEnumerable<BackupChainInfo> GetBackupChainInfos()
        {
            return this.backupChainInfos;
        }

        IList<BackupChainInfo> backupChainInfos = new List<BackupChainInfo>();
    }
}
