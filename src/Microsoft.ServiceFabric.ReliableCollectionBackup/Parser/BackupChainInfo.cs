// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    internal class BackupChainInfo
    {
        private List<string> backupDirectories;
        private readonly string commonRootFolder;

        public string CommonRootFolder
        {
            get { return this.commonRootFolder; }
            private set { }
        }

        public BackupChainInfo(List<string> backupDirectories)
        {
            this.backupDirectories = backupDirectories;
            this.commonRootFolder = this.FindCommonRootFolder();
        }

        private string FindCommonRootFolder()
        {
            if (this.backupDirectories.Count == 0)
            {
                throw new ArgumentException("Need to provide atleast one backup directory");
            }

            foreach (var dir in this.backupDirectories)
            {
                if (!Directory.Exists(dir))
                {
                    throw new DirectoryNotFoundException(dir);
                }
            }

            return this.backupDirectories[0];
        }
    }
}
