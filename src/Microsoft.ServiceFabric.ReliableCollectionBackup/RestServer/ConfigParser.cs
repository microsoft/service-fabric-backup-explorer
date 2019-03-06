// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    /// <summary>
    /// Parser for RestServer configuration file.
    /// <see cref="Configuration"/> class for parsed Configuration object.
    /// Format of file :
    /// {
    ///     "BackupChainInfos" : [
    ///         {
    ///             "AppName" : "MyAppName",
    ///             "ServiceName" : "MyServiceName",
    ///             "BackupChainPath" : "../Parser.Tests/UserFullBackup/",
    ///                 "CodePackagePath" : "../Parser.Tests/UserType/bin/",
    ///                 "Serializers" : [ {
    ///                     "StateFullyQualifiedTypeName": "Microsoft.ServiceFabric.ReliableCollectionBackup.UserType.User, Microsoft.ServiceFabric.ReliableCollectionBackup.UserType, Version=1.0.0.0, Culture=neutral, PublicKeyToken=365143bb27e7ac8b",
    ///                     "SerializerFullyQualifiedTypeName": "Microsoft.ServiceFabric.ReliableCollectionBackup.UserType.UserSerializer, Microsoft.ServiceFabric.ReliableCollectionBackup.UserType, Version=1.0.0.0, Culture=neutral, PublicKeyToken=365143bb27e7ac8b"
    ///                     } ]
    ///         },
    ///         ...
    ///     ]
    /// }
    /// </summary>
    internal class ConfigParser
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configPath">Path of json config file</param>
        public ConfigParser(string configPath)
        {
            using (var stream = File.OpenRead(configPath))
            {
                this.Parse(stream);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configStream">Json stream of configuration file.</param>
        public ConfigParser(Stream configStream)
        {
            this.Parse(configStream);
        }

        /// <summary>
        /// Gives Configration after parsing from configuration file.
        /// </summary>
        /// <returns>Configration after parsing json config</returns>
        public Configuration GetConfigration()
        {
            return this.configuration;
        }

        private void Parse(Stream jsonStream)
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

        private void Validate(IList<BackupChainInfo> backupChainInfos)
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

        private Configuration configuration;
        private const string BackupChainInfosKey = "BackupChainInfos";
    }
}
