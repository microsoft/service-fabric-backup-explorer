// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;
using Newtonsoft.Json;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Controllers
{
    /// <summary>
    /// TransactionController for iterating through transactions of the backup.
    /// </summary>
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        public TransactionsController(BackupParserWrapper backupParserWrapper)
        {
            this.backupParserWrapper = backupParserWrapper;
        }

        [HttpGet("/api/transactions/next", Name = "GetNextTransaction")]
        public IActionResult GetNextTransaction(int? count)
        {
            int queryCount = 1;
            if (count != null)
            {
                queryCount = count.Value;
            }

            // cases : 
            // 1. Parsing has finished and no next transactions.
            // 2. Parsing has not finished but still loading full backup so no transaction seen.
            // 3. Parsing has not finished and we have n transactions to give.

            // case 1.
            if (this.backupParserWrapper.HasParsingFinished() && 
                !this.backupParserWrapper.HasNextTransaction())
            {
                var finishResponse = new TransactionsResponse("finished",
                    "No more transaction in backup",
                    null);

                return new JsonResult(finishResponse);
            }

            // case 2.
            if (!this.backupParserWrapper.HasParsingFinished() &&
                !this.backupParserWrapper.HasNextTransaction())
            {
                var inProgressResponse = new TransactionsResponse("inprogress",
                    "No transaction seen till now since last time.",
                    null);

                return new JsonResult(inProgressResponse);
            }

            // case 3.
            var transactions = this.backupParserWrapper.TryGetTransactions(queryCount);
            var reason = "";
            if (transactions.Count < queryCount)
            {
                reason = $"Only ${transactions.Count} transactions in queue are present at this time.";
            }

            var response = new TransactionsResponse("success", reason, transactions);
            return new JsonResult(response);
        }

        private BackupParserWrapper backupParserWrapper;
    }
}
