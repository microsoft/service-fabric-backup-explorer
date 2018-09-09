using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
