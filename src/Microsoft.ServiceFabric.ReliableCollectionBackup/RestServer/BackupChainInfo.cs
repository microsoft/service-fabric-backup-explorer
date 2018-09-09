using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    internal class BackupChainInfo
    {
        public BackupChainInfo(
            string appName,
            string serviceName,
            string backupChainPath,
            string codePackagePath,
            IEnumerable<SerializerInfo> serializers)
        {
            this.AppName = appName;
            this.ServiceName = serviceName;
            this.BackupChainPath = backupChainPath;
            this.CodePackagePath = codePackagePath;
            this.Serializers = serializers ?? Enumerable.Empty<SerializerInfo>();
        }

        public string AppName { get; set; }
        public string ServiceName { get; set; }
        public string BackupChainPath { get; set; }
        public string CodePackagePath { get; set; }
        public IEnumerable<SerializerInfo> Serializers { get; set; }

        internal void ValidateAndSetDefaultValues()
        {
            if (String.IsNullOrWhiteSpace(this.BackupChainPath))
            {
                throw new InvalidDataException("Validation failed : BackupChainPath is a required field");
            }

            if (String.IsNullOrWhiteSpace(this.CodePackagePath))
            {
                throw new InvalidDataException("Validation failed : CodePackagePath is a required field");
            }

            foreach (var serializer in this.Serializers)
            {
                serializer.Validate();
            }

            if (String.IsNullOrWhiteSpace(this.AppName))
            {
                this.AppName = Guid.NewGuid().ToString("N");
            }

            if (String.IsNullOrWhiteSpace(this.ServiceName))
            {
                this.ServiceName = Guid.NewGuid().ToString("N");
            }
        }
    }
}
