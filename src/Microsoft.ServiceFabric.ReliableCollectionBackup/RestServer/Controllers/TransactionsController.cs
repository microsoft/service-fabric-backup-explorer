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
            if (count == null)
            {
                count = 1;
            }

            // cases : 
            // 1. Parsing has finished and no next transactions.
            // 2. Parsing has not finished but still loading full backup so no transaction seen.
            // 3. Parsing has not finished and we have n transactions to give.

            // case 1.
            if (this.backupParserWrapper.HasParsingFinished() && 
                !this.backupParserWrapper.HasNextTransaction())
            {
                return new JsonResult(new Dictionary<string, string>()
                {
                    { "status", "finished" },
                    { "reason", "No more transaction in backup" },
                    { "numTransactions", "0" }
                });
            }

            // case 2.
            if (!this.backupParserWrapper.HasParsingFinished() &&
                !this.backupParserWrapper.HasNextTransaction())
            {
                return new JsonResult(new Dictionary<string, string>()
                {
                    { "status", "inprogress"},
                    { "reason", "No transaction seen till now since last time." },
                    { "numTransactions", "0" }
                });
            }

            // case 3.
            var transactions = new List<NotifyTransactionAppliedEventArgs>(count.Value);
            for (int i = 0; i < count; ++i)
            {
                if (this.backupParserWrapper.HasNextTransaction())
                {
                    transactions.Add(this.backupParserWrapper.GetNextTransaction());
                }
                else
                {
                    break;
                }
            }

            return new JsonResult(new Dictionary<string, string>()
            {
                { "status", "inprogress"},
                { "reason", "No transaction seen till now." },
                { "numTransactions", $"{transactions.Count}" },
                { "transactions", $"{JsonConvert.SerializeObject(transactions)}" }
            });
        }

        private BackupParserWrapper backupParserWrapper;
    }
}
