// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    /// <summary>
    /// Wrapper around BackupParser for enumerating over transactions.
    /// </summary>
    public class BackupParserManager : IDisposable
    {
        public BackupParserManager(BackupParser backupParser)
        {
            this.BackupParser = backupParser;
            // todo : take NumMaxTransactionsInMemory from config.
            this.transactionsQueue = new BlockingCollection<NotifyTransactionAppliedEventArgs>(NumMaxTransactionsInMemory);
            // todo : take boolean flag to block on transactions or not.
            this.BackupParser.TransactionApplied += (sender, args) =>
            {
                transactionsQueue.Add(args);
            };
        }

        /// <summary>
        /// Starts parsing the backup without blocking.
        /// </summary>
        /// <returns>Task representing the asynchronous parsing of backup.</returns>
        public Task StartParsing()
        {
            if (this.parsingTask == null)
            {
                // todo : take cancellation timeout from config.
                this.parsingTask = this.BackupParser.ParseAsync(CancellationToken.None);
            }
            return parsingTask;
        }

        /// <summary>
        /// Try to give <paramref name="count" /> number of transactions.
        /// If less than <paramref name="count"/> number of transactions are currently collected,
        /// then return only that many.
        /// </summary>
        /// <param name="count">Number of transactions to give.</param>
        /// <returns>Returns list of <paramref name="count"/> or less number of transactions</returns>
        public List<NotifyTransactionAppliedEventArgs> TryGetTransactions(int count)
        {
            lock(queueLock)
            {
                var toTake = Math.Min(transactionsQueue.Count, count);
                var transactions = new List<NotifyTransactionAppliedEventArgs>();

                for (int i = 0; i < toTake; ++i)
                {
                    transactions.Add(transactionsQueue.Take());
                }

                return transactions;
            }
        }

        /// <summary>
        /// Checks if it has any unconsumed transactions.
        /// </summary>
        /// <returns>True if we have more transactions otherwise false.</returns>
        public bool HasNextTransaction()
        {
            return this.transactionsQueue.Count > 0;
        }

        /// <summary>
        /// Checks if parsing operation has finished.
        /// If this parsing has finished and <see cref="HasNextTransaction"/> also returns false,
        /// then we will not have any more transactions in future.
        /// </summary>
        /// <returns></returns>
        public bool HasParsingFinished()
        {
            return this.parsingTask.IsCompleted;
        }

        /// <summary>
        /// Dispose the Backup manager
        /// </summary>
        public void Dispose()
        {
            this.BackupParser.Dispose();
        }

        internal BackupParser BackupParser { get; private set; }
        private Task parsingTask;
        private BlockingCollection<NotifyTransactionAppliedEventArgs> transactionsQueue;
        // lock is used for taking N items from transactionsQueue atomically across concurrent requests.
        private static Object queueLock = new Object();
        private const int NumMaxTransactionsInMemory = 1000;
    }
}
