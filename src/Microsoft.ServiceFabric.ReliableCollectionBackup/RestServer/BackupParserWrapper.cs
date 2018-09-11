using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    /// <summary>
    /// Wrapper around BackupParser for enumerating over transactions.
    /// </summary>
    public class BackupParserWrapper
    {
        public BackupParserWrapper(BackupParser backupParser)
        {
            this.BackupParser = backupParser;
            this.transactionsQueue = new BlockingCollection<NotifyTransactionAppliedEventArgs>(NumMaxTransactionsInMemory);
            this.BackupParser.TransactionApplied += (sender, args) =>
            {
                transactionsQueue.Add(args);                
            };
        }

        public Task StartParsing()
        {
            if (this.parsingTask == null)
            {
                this.parsingTask = this.BackupParser.ParseAsync(CancellationToken.None);
            }
            return parsingTask;
        }

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

        public bool HasNextTransaction()
        {
            return this.transactionsQueue.Count > 0;
        }

        public bool IsReadyForWriting()
        {
            return this.HasParsingFinished();
        }

        public BackupParser BackupParser { get; private set; }

        public bool HasParsingFinished()
        {
            return this.parsingTask.IsCompleted;
        }

        private Task parsingTask;
        private BlockingCollection<NotifyTransactionAppliedEventArgs> transactionsQueue;
        private const int NumMaxTransactionsInMemory = 1000;
        // use for taking N items from transactionsQueue across concurrent requests.
        private static Object queueLock = new Object();
    }
}
