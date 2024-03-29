// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using log4net;
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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Constructor of TransactionChangeManager.
        /// </summary>
        public TransactionChangeManager(ReliabilitySimulator reliabilitySimulator)
        {
            this.reliabilitySimulator = reliabilitySimulator;
            this.reliableCollectionsChanges = new ConcurrentDictionary<Uri, ReliableCollectionChange>();
        }

        /// <summary>
        /// Adds changes in the Reliable collections that take place.
        /// </summary>
        /// <param name="reliableCollectionName">Name of Reliable Collection which changed in this transaction.</param>
        /// <param name="changes">Changes in the Reliable collections after the checkpoint.</param>
        public void CollectChanges(Uri reliableCollectionName, EventArgs changes)
        {
            if (!this.reliableCollectionsChanges.ContainsKey(reliableCollectionName))
            {
                this.reliableCollectionsChanges[reliableCollectionName] = new ReliableCollectionChange(reliableCollectionName);
            }

            this.reliableCollectionsChanges[reliableCollectionName].Changes.Enqueue(changes);
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
            log.Info($" StateManagerChanged with event Action:- {e.Action}");
            var rebuildEvent = e as NotifyStateManagerRebuildEventArgs;
            if (rebuildEvent != null)
            {
                var reliableStates = rebuildEvent.ReliableStates.ToEnumerable();
                foreach (var reliableState in reliableStates)
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
                this.reliabilitySimulator.GrantReadAccess();
                return;
            }

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
                    {
                        log.Info("Queue  is present in the Backup. Backup Viewer cannot handle these events");
                        break;
                    }
                case ReliableStateKind.ReliableConcurrentQueue:
                    {
                        log.Info("Concurrent Queue is present in the backup. Backup Viewer cannot handle these events.");
                        break;
                    }
                default:
                    break;
            }



        }

        private void AddDictionaryChangedHandler<TKey, TValue>(IReliableDictionary<TKey, TValue> dictionary)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            log.Info($"AddDictionaryChangeHandler for Reliable Dictionary: {dictionary.Name}");
            dictionary.DictionaryChanged += this.OnDictionaryChanged;
        }

        internal void OnDictionaryChanged<TKey, TValue>(object sender, NotifyDictionaryChangedEventArgs<TKey, TValue> e)
        {
            var reliableState = sender as IReliableState;
            log.Info($"DictionaryChange Event for {reliableState.Name} with Event :- {e.Action}");
            this.CollectChanges(reliableState.Name, e);           

        }

        private ConcurrentDictionary<Uri, ReliableCollectionChange> reliableCollectionsChanges;
        
        private ReliabilitySimulator reliabilitySimulator;
    }
}