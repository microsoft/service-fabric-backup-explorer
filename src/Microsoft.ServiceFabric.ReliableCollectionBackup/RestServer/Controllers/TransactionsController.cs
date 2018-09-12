// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

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
            if (this.backupParserManager.HasParsingFinished() &&
                !this.backupParserManager.HasNextTransaction())
            {
                var finishResponse = new TransactionsResponse("finished",
                    "No more transaction in backup",
                    null);

                return new JsonResult(finishResponse);
            }

            // case 2.
            if (!this.backupParserManager.HasParsingFinished() &&
                !this.backupParserManager.HasNextTransaction())
            {
                var inProgressResponse = new TransactionsResponse("inprogress",
                    "No transaction seen till now since last time.",
                    null);

                return new JsonResult(inProgressResponse);
            }

            // case 3.
            var transactions = this.backupParserManager.TryGetTransactions(queryCount);
            var reason = "";
            if (transactions.Count < queryCount)
            {
                reason = $"Only ${transactions.Count} transactions are collected till this time.";
            }

            var response = new TransactionsResponse("success", reason, transactions);
            return new JsonResult(response);
        }

        private BackupParserManager backupParserManager;
    }
}
