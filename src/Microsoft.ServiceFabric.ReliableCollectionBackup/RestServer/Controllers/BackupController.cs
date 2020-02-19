// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Controllers
{
    /// <summary>
    /// BackupController for taking full/incremental backups.
    /// </summary>
    [ApiController]
    public class BackupController : ControllerBase
    {
        public BackupController(BackupParserManager backupParserManager)
        {
            this.backupParserManager = backupParserManager;
        }

        [HttpPost("/backup/full", Name = "PostFullBackup")]
        public async Task<IActionResult> PostFullBackup([FromBody] BackupRequestBody backupRequest)
        {
            return await TakeBackup(backupRequest, BackupOption.Full);
        }

        [HttpPost("/backup/incremental", Name = "PostIncrementalBackup")]
        public async Task<IActionResult> PostIncrementalBackup([FromBody] BackupRequestBody backupRequest)
        {
            return await TakeBackup(backupRequest, BackupOption.Incremental);
        }

        private async Task<IActionResult> TakeBackup(BackupRequestBody backupRequest, Data.BackupOption backupOption)
        {
            var error = this.ValidateRequest(backupRequest);
            if (!error.IsValid)
            {
                return BadRequest(error);
            }

            if (!this.backupParserManager.HasParsingFinished())
            {
                Response.StatusCode = 423; // resource locked at this time.
                return new JsonResult(new Dictionary<string, string>()
                {
                    { "status", "inprogress" },
                    { "reason", "Backup parsing is still going on. " }
                });
            }

            var timeout = TimeSpan.FromSeconds(backupRequest.TimeoutInSecs);
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(backupRequest.TimeoutInSecs));
            var cancellationToken = cancellationTokenSource.Token;
            this.UserBackupLocation = String.IsNullOrWhiteSpace(backupRequest.BackupLocation) ?
                Directory.GetCurrentDirectory() : backupRequest.BackupLocation;

            await this.backupParserManager.BackupParser.BackupAsync(backupOption, timeout, cancellationToken, this.OnBackupCompletionAsync);
            return new JsonResult(new Dictionary<string, string>()
            {
                { "status", "success" },
                { "backPath", this.BackupPath },
            });
        }

        private ModelStateDictionary ValidateRequest(BackupRequestBody backupRequest)
        {
            var error = new ModelStateDictionary();
            if (backupRequest.CancellationTokenInSecs == 0)
            {
                error.AddModelError("CancellationTokenInSecs", new MissingFieldException("CancellationTokenInSecs is a required argument"));
            }

            if (backupRequest.TimeoutInSecs == 0)
            {
                error.AddModelError("TimeoutInSecs", new MissingFieldException("TimeoutInSecs is a required argument"));
            }

            return error;
        }

        private async Task<bool> OnBackupCompletionAsync(BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            this.BackupPath = Path.Combine(this.UserBackupLocation, Guid.NewGuid().ToString("N"));
            await Utilities.CopyDirectory(backupInfo.Directory, this.BackupPath);
            return true;
        }

        /// <summary>
        /// BackupParserManager on which to invoke backup apis.
        /// </summary>
        private BackupParserManager backupParserManager;

        /// <summary>
        /// Backup folder location under <see cref="UserBackupLocation" />
        /// </summary>
        private string BackupPath;

        /// <summary>
        /// User supplied backup folder location.
        /// </summary>
        private string UserBackupLocation;
    }
}
