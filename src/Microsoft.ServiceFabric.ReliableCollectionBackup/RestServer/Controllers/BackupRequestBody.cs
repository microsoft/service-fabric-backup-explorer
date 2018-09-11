// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Controllers
{
    /// <summary>
    /// Request body for Backup api requests.
    /// </summary>
    public class BackupRequestBody
    {
        /// <summary>
        /// Timeout of Cancellation token.
        /// </summary>
        [Required]
        [JsonRequired]
        public uint CancellationTokenInSecs { get; set; }

        /// <summary>
        /// Timeout passed in Backup Apis.
        /// </summary>
        [Required]
        [JsonRequired]
        public uint TimeoutInSecs { get; set; }

        /// <summary>
        /// Location where to save backup.
        /// </summary>
        [Required]
        [JsonRequired]
        public string BackupLocation { get; set; }
    }
}