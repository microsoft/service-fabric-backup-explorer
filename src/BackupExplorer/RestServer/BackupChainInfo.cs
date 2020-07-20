// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    /// <summary>
    /// Information about a backup chain in the Rest Server configuration file.
    /// <see cref="Configuration"/> for full config file.
    /// </summary>
    internal class BackupChainInfo
    {
        /// <summary>
        /// Constructor of BackupChainInfo
        /// </summary>
        /// <param name="appName">AppName to use for rest endpoint</param>
        /// <param name="serviceName">ServiceName to use for rest endpoint</param>
        /// <param name="backupChainPath">Location of backup chain.</param>
        /// <param name="codePackagePath">Location of code package to use for loading <paramref name="backupChainPath"/>.</param>
        /// <param name="serializers">List of serializers with fully qualified names in <paramref name="codePackagePath"/>.</param>
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

        /// <summary>
        /// AppName to use for rest endpoint. http://localhost:5000/<AppName><ServiceName>
        /// </summary>
        public string AppName { get; set; }
        /// <summary>
        /// ServiceName to use for rest endpoint. http://localhost:5000/<AppName><ServiceName>
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// Location of backup chain.
        /// </summary>
        public string BackupChainPath { get; set; }
        /// <summary>
        /// Location of code package to use for loading <see cref="BackupChainPath"/>
        /// </summary>
        public string CodePackagePath { get; set; }
        /// <summary>
        /// List of serializers with fully qualified names in <see cref="CodePackagePath"/>
        /// </summary>
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
