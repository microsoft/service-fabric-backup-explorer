using System;
using System.Collections.Generic;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    internal class TransactionChangeManager
    {
        public TransactionChangeManager()
        {
            this.reliableCollectionChanges = new Dictionary<Uri, ReliableCollectionChange>();
        }

        /// <summary>
        /// Add a new change in the transaction.
        /// </summary>
        /// <param name="reliableCollectionName"></param>
        /// <param name="changes"></param>
        public void CollectChanges(Uri reliableCollectionName, EventArgs changes)
        {
            if (!this.reliableCollectionChanges.ContainsKey(reliableCollectionName))
            {
                this.reliableCollectionChanges[reliableCollectionName] = new ReliableCollectionChange(reliableCollectionName);
            }

            this.reliableCollectionChanges[reliableCollectionName].Changes.Add(changes);
        }

        /// <summary>
        /// Clear the changes in the transaction.
        /// </summary>
        public void TransactionCompleted()
        {
            this.reliableCollectionChanges.Clear();
        }

        /// <summary>
        /// Shows the changes collection till now.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReliableCollectionChange> ShowChanges()
        {
            return this.reliableCollectionChanges.Values;
        }

        private Dictionary<Uri, ReliableCollectionChange> reliableCollectionChanges;
    }
}
