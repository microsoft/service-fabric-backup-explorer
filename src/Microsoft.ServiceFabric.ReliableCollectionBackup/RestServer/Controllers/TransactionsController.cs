// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Controllers
{
    /// <summary>
    /// TransactionController for iterating through transactions of the backup.
    /// </summary>
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        public TransactionsController(BackupParserManager backupParserManager)
        {
            this.backupParserManager = backupParserManager;
        }

        [HttpGet("/api/transactions", Name = "GetAllTransaction")]
        public IActionResult GetAllTransactions()
        {
            var transactions = this.backupParserManager.TryGetTransactions();
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            JsonResult response =  new JsonResult(transactions, jsonSerializerSettings);
            return response;
        }

        private BackupParserManager backupParserManager;
    }
}
