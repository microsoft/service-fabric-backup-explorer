// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;
using Microsoft.ServiceFabric.Tools.ReliabilitySimulator;

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
        public TransactionChangeManager(ReliabilitySimulator reliabilitySimulator)
        {
            this.reliableCollectionsChanges = new Dictionary<Uri, ReliableCollectionChange>();
            this.reliabilitySimulator = reliabilitySimulator;
        }

        /// <summary>
        /// Add a new change in the transaction.
        /// </summary>
        /// <param name="reliableCollectionName">Name of Reliable Collection which changed in this transaction.</param>
        /// <param name="changes">Changes in ReliableCollection.</param>
        public void CollectChanges(Uri reliableCollectionName, EventArgs changes)
        {
            string dictName = reliableCollectionName.ToString();
            dictName.Replace('.', '_');
            dictionaryNameMap.Add(reliableCollectionName, dictName);

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
            this.reliableCollectionsChanges = new Dictionary<Uri, ReliableCollectionChange>();
        }

        /// <summary>
        /// Gets Reliable Collection changes collected till now.
        /// </summary>
        /// <returns>All reliable collection changes collected.</returns>
        public IEnumerable<ReliableCollectionChange> GetAllChanges()
        {
            return this.reliableCollectionsChanges.Values;
        }

        /// <summary>
        /// Listens for StateManager change events to hook into ReliableStates for any change events.
        /// These change events are then collected to show when the Transaction is committed.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">StateManager change event arguments.</param>
        internal void OnStateManagerChanged(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Rebuild)
            {
                this.ProcessStateManagerRebuildNotification(e);
                return;
            }
            this.ProcessStateManagerSingleEntityNotification(e);
        }

        private void ProcessStateManagerSingleEntityNotification(NotifyStateManagerChangedEventArgs e)
        {
            var addoperation = e as NotifyStateManagerSingleEntityChangedEventArgs;
            var reliableStateType = addoperation.ReliableState.GetType();
            switch (ReliableStateKindUtils.KindOfReliableState(addoperation.ReliableState))
            {
                case ReliableStateKind.ReliableDictionary:
                    {
                        var keyType = reliableStateType.GetGenericArguments()[0];
                        var valueType = reliableStateType.GetGenericArguments()[1];

                        // use reflection to call my own method because key/value types are known at runtime.
                        this.GetType().GetMethod("AddDictionaryChangedHandler", BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(keyType, valueType)
                            .Invoke(this, new object[] { addoperation.ReliableState });
                        break;
                    }

                case ReliableStateKind.ReliableQueue:
                case ReliableStateKind.ReliableConcurrentQueue:
                default:
                    break;
            }
        }

        private void ProcessStateManagerRebuildNotification(NotifyStateManagerChangedEventArgs e)
        {
            var rebuildEvent = e as NotifyStateManagerRebuildEventArgs;
            if (rebuildEvent != null)
            {
                foreach (var reliableState in rebuildEvent.ReliableStates.ToEnumerable())
                {
                    var reliableStateType1 = reliableState.GetType();
                    switch (ReliableStateKindUtils.KindOfReliableState(reliableState))
                    {
                        case ReliableStateKind.ReliableDictionary:
                            {
                                var keyType = reliableStateType1.GetGenericArguments()[0];
                                var valueType = reliableStateType1.GetGenericArguments()[1];
                                // use reflection to call my own method because key/value types are known at runtime.
                                this.GetType().GetMethod("AddDictionaryChangedHandler", BindingFlags.Instance | BindingFlags.NonPublic)
                                    .MakeGenericMethod(keyType, valueType)
                                    .Invoke(this, new object[] { reliableState });
                                break;
                            }

                        case ReliableStateKind.ReliableQueue:
                        case ReliableStateKind.ReliableConcurrentQueue:
                        default:
                            break;
                    }


                }
                //reliabilitySimulator.GrantReadAccess();
                return;
            }
            return;
        }

        private void AddDictionaryChangedHandler<TKey, TValue>(IReliableDictionary<TKey, TValue> dictionary)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            dictionary.DictionaryChanged += this.OnDictionaryChanged;
            //dictionary.RebuildNotificationAsyncCallback = OnDictionaryRebuildNotificationHandlerAsync;

        }

        private async Task OnDictionaryRebuildNotificationHandlerAsync<TKey, TValue>(IReliableDictionary<TKey,TValue> sender, NotifyDictionaryRebuildEventArgs<TKey, TValue> e)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var reliableState = sender as IReliableState;
            var enumerator = e.State.GetAsyncEnumerator();
            this.CollectChanges(reliableState.Name, e);
            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                Console.WriteLine(enumerator.Current.Key);
            }

        }

        internal void OnDictionaryChanged2<TKey, TValue>(object sender, NotifyDictionaryRebuildEventArgs<TKey, TValue> e)
        {

        }
            
        internal void OnDictionaryChanged<TKey, TValue>(object sender, NotifyDictionaryChangedEventArgs<TKey, TValue> e)
        {
            var reliableState = sender as IReliableState;
            var keyAddArgs = e as NotifyDictionaryItemAddedEventArgs<TKey, TValue>;
            if (keyAddArgs != null)
            {
                this.CollectChanges(reliableState.Name, e);
            }            
            var removekeyAddArgs = e as NotifyDictionaryItemRemovedEventArgs<TKey, TValue>;
            if (removekeyAddArgs!= null)
            {
                this.CollectChanges(reliableState.Name, e);
            }

            var updatekeyArgs = e as NotifyDictionaryItemUpdatedEventArgs<TKey, TValue>;
            if (updatekeyArgs != null)
            {
                this.CollectChanges(reliableState.Name, e);
            }
            var clearkeyArgs = e as NotifyDictionaryClearEventArgs<TKey, TValue>;
            if (clearkeyArgs != null)
            {
                this.CollectChanges(reliableState.Name, e);
            }            
        }

        private Dictionary<Uri, ReliableCollectionChange> reliableCollectionsChanges;
        private ReliabilitySimulator reliabilitySimulator;
        public Dictionary<Uri, string> dictionaryNameMap = new Dictionary<Uri, string>();
    }
}
