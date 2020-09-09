// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    /// <summary>
    /// Rest Server Configurations. 
    /// It includes:
    /// 1. List of backup chain infos.
    /// </summary>
    internal class Configuration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="backupChainInfos">List of <see cref="BackupChainInfo" /></param>
        public Configuration(IList<BackupChainInfo> backupChainInfos)
        {
            this.backupChainInfos = backupChainInfos;
        }

        /// <summary>
        /// Get list of <see cref="BackupChainInfo"/> in the configuration.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BackupChainInfo> GetBackupChainInfos()
        {
            return this.backupChainInfos;
        }

        private IList<BackupChainInfo> backupChainInfos = new List<BackupChainInfo>();
    }
}
