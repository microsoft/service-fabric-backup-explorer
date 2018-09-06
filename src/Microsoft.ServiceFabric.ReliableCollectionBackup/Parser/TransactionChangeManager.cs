// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// TransactionChangeManager keeps track of changes in Reliable Collections of a replica in a transaction.
    /// </summary>
    internal class TransactionChangeManager
    {
        /// <summary>
        /// Constructor of TransactionChangeManager.
        /// </summary>
        public TransactionChangeManager()
        {
            this.reliableCollectionsChanges = new Dictionary<Uri, ReliableCollectionChange>();
        }

        /// <summary>
        /// Add a new change in the transaction.
        /// </summary>
        /// <param name="reliableCollectionName">Name of Reliable Collection which changed in this transaction.</param>
        /// <param name="changes">Changes in ReliableCollection.</param>
        public void CollectChanges(Uri reliableCollectionName, EventArgs changes)
        {
            if (!this.reliableCollectionsChanges.ContainsKey(reliableCollectionName))
            {
                this.reliableCollectionsChanges[reliableCollectionName] = new ReliableCollectionChange(reliableCollectionName);
            }

            this.reliableCollectionsChanges[reliableCollectionName].Changes.Add(changes);
        }

        /// <summary>
        /// Clears the changes in the transaction to get prepared for next transaction.
        /// </summary>
        public void TransactionCompleted()
        {
            this.reliableCollectionsChanges.Clear();
        }

        /// <summary>
        /// Gets Reliable Collection changes collected till now.
        /// </summary>
        /// <returns>All reliable collection changes collected.</returns>
        public IEnumerable<ReliableCollectionChange> GetAllChanges()
        {
            return this.reliableCollectionsChanges.Values;
        }

        internal void OnDictionaryChanged<TKey, TValue>(object sender, NotifyDictionaryChangedEventArgs<TKey, TValue> e)
        {
            var keyAddArgs = e as NotifyDictionaryItemAddedEventArgs<TKey, TValue>;
            var reliableState = sender as IReliableState;

            if (null == keyAddArgs || null == reliableState)
            {
                // log here.
                return;
            }

            this.CollectChanges(reliableState.Name, e);
        }

        private Dictionary<Uri, ReliableCollectionChange> reliableCollectionsChanges;
    }
}
