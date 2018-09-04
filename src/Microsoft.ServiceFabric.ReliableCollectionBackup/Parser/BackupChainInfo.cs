using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.ServiceFabric.Tools.RCBackupParser
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
