// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;

using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Controllers
{
    /// <summary>
    /// Response class for Api : /api/transactions/next
    /// </summary>
    internal class TransactionsResponse
    {
        public TransactionsResponse(string status, string reason, List<NotifyTransactionAppliedEventArgs> transactions)
        {
            this.status = status;
            this.reason = reason;
            this.transactions = transactions;
        }

        /// <summary>
        /// status represents the success/inprogress/finished status of api related to transaction stream.
        /// 'success' : if user's api request succedded, we return 'success'.
        /// 'finished' : if user's api request can be processed as transaction stream is
        ///     consumed completely, we return 'finished'.
        /// 'inprogress' : if we can't return any transaction in this request,
        ///     we return 'inprogress' status for customer to retry again.
        /// </summary>
        public string status;

        /// <summary>
        /// reason gives more information related to api response for debugging.
        /// </summary>
        public string reason;

        /// <summary>
        /// List of transactions asked by transaction api
        /// </summary>
        public List<NotifyTransactionAppliedEventArgs> transactions;
    }
}
