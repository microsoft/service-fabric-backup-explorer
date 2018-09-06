// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Fabric;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;
using Microsoft.ServiceFabric.Tools.ReliabilitySimulator;
using Microsoft.ServiceFabric.Replicator;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// Actual implementation of BackupParserImpl.
    /// </summary>
    internal class BackupParserImpl : IDisposable
    {
        /// <summary>
        /// Constructor for BackupParserImpl.
        /// </summary>
        /// <param name="backupChainPath">Folder path that contains sub folders of one full and multiple incremental backups.</param>
        /// <param name="codePackagePath">Code packages of the service whose backups are provided in <paramref name="backupChainPath" />.</param>
        public BackupParserImpl(string backupChainPath, string codePackagePath)
        {
            this.backupChainPath = backupChainPath;
            this.codePackage = new CodePackageInfo(codePackagePath);
            this.workFolder = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());

            Console.WriteLine("Work Folder : {0}", this.workFolder);

            Directory.CreateDirectory(this.workFolder);

            this.reliabilitySimulator = new ReliabilitySimulator(
                this.workFolder,
                new Uri("fabric:/rcbackupapp/rcbackupservice"),
                this.OnDataLossCallback,
                this.CreateStateProvider);
            this.reliabilitySimulator.CreateReplica(true, false);

            this.stateManager = new StateManager(reliabilitySimulator);
            this.seenFirstTransaction = false;
            this.transactionChangeManager = new TransactionChangeManager();
        }

        /// <summary>
        /// Events that notifies about the committed transactions.
        /// </summary>
        public event EventHandler<NotifyTransactionAppliedEventArgs> TransactionApplied;

        /// <summary>
        /// Parses the backup.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop backup parsing.</param>
        /// <returns>Task associated with parsing.</returns>
        public async Task ParseAsync(CancellationToken cancellationToken)
        {
            // Since these are COM calls, we need to wrap them to run in MTA.
            // asnegi: I am still not sure why we need this even when lower native layer has a similar wrapper.
            await ComMtaHelper.WrapNativeSyncInvokeInMTA(async () =>
            {
                var transactionalReplicatorSettings = TransactionalReplicatorSettingsHelper.Create(
                    this.workFolder,
                    "Ktl",
                    checkpointThresholdMB: 200, // keep these settings configurable for the user to play for performance.
                    useDefaultSharedLogId: true,
                    LogManagerLoggerType : System.Fabric.Data.Log.LogManager.LoggerType.Inproc);

                await this.reliabilitySimulator.OpenReplicaWithSettingsAsync(ReplicaOpenMode.New, transactionalReplicatorSettings).ConfigureAwait(false);
                await this.reliabilitySimulator.PromoteReplicaAsync(false).ConfigureAwait(false);
                await this.reliabilitySimulator.PrepareForDataLossAsync().ConfigureAwait(false);

                this.Replicator.TransactionChanged += this.OnTransactionChanged;
                this.Replicator.StateManager.StateManagerChanged += this.OnStateManagerChanged;

                await this.reliabilitySimulator.OnDataLossAsync(cancellationToken).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// StateManager associated with this Parser.
        /// </summary>
        public StateManager StateManager
        {
            get { return this.stateManager; }
            internal set { this.stateManager = value; }
        }

        /// <summary>
        /// Creates a backup of current replica.
        /// </summary>
        /// <param name="backupCallback">Backup callback to trigger at finish of Backup operation.</param>
        /// <param name="backupOption">The type of backup to perform.</param>
        /// <param name="timeout">The timeout for this operation.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Task representing this asynchronous backup operation.</returns>
        public async Task BackupAsync(Func<BackupInfo, CancellationToken, Task<bool>> backupCallback, BackupOption backupOption, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await this.Replicator.BackupAsync(backupCallback, backupOption, timeout, cancellationToken);
        }

        /// <summary>
        /// Listens for StateManager change events to hook into ReliableStates for any change events.
        /// These change events are then collected to show when the Transaction is committed.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">StateManager change event arguments.</param>
        private void OnStateManagerChanged(object sender, NotifyStateManagerChangedEventArgs e)
        {
            var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
            if (null == operation)
            {
                return;
            }

            if (operation.Action == NotifyStateManagerChangedAction.Add)
            {
                var reliableStateType = operation.ReliableState.GetType();
                switch (ReliableStateKindUtils.KindOfReliableState(operation.ReliableState))
                {
                    case ReliableStateKind.ReliableDictionary:
                        {
                            var keyType = reliableStateType.GetGenericArguments()[0];
                            var valueType = reliableStateType.GetGenericArguments()[1];

                            // use reflection to call my own method because key/value types are known at runtime.
                            this.GetType().GetMethod("AddDictionaryChangedHandler", BindingFlags.Instance | BindingFlags.NonPublic)
                                .MakeGenericMethod(keyType, valueType)
                                .Invoke(this, new object[] { operation.ReliableState });
                            break;
                        }
                    case ReliableStateKind.ReliableQueue:
                    case ReliableStateKind.ReliableConcurrentQueue:
                    default:
                        break;
                }
            }
        }

        private void AddDictionaryChangedHandler<TKey, TValue>(IReliableDictionary<TKey, TValue> dictionary)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            dictionary.DictionaryChanged += this.transactionChangeManager.OnDictionaryChanged;
        }

        private void OnTransactionChanged(object sender, NotifyTransactionChangedEventArgs e)
        {
            if (e.Action == NotifyTransactionChangedAction.Commit)
            {
                // If this is first transaction we have seen, open Replica for reads
                // before notifying about committed transactions.
                if (!this.seenFirstTransaction)
                {
                    this.SetUpReplicaForReads();
                    this.seenFirstTransaction = true;
                }

                this.OnTransactionCommitted(sender, e);
                this.transactionChangeManager.TransactionCompleted();
            }
        }

        private void OnTransactionCommitted(object sender, NotifyTransactionChangedEventArgs e)
        {
            var transactionApplied = this.TransactionApplied;
            if (null == transactionApplied)
            {
                return;
            }

            try
            {
                var transactionAppliedEventArgs = new NotifyTransactionAppliedEventArgs(
                    e.Transaction,
                    this.transactionChangeManager.GetAllChanges());

                transactionApplied.Invoke(this, transactionAppliedEventArgs);
            }
            catch (Exception)
            {
                // log the exception..
            }
        }

        private async Task<bool> OnDataLossCallback(CancellationToken cancellationToken)
        {
            RestorePolicy restorePolicy = RestorePolicy.Safe;
            await this.Replicator.RestoreAsync(this.backupChainPath, restorePolicy, cancellationToken).ConfigureAwait(false);
            return true;
        }

        public void Dispose()
        {
            // delete replica.
            this.reliabilitySimulator.DropReplicaAsync().GetAwaiter().GetResult();
            // delete work foler.
            if (Directory.Exists(this.workFolder))
            {
                Directory.Delete(this.workFolder, true);
            }
            // remove assemblyresolver.
            this.codePackage.Dispose();
        }

        internal StatefulServiceContext GetStatefulServiceContext()
        {
            return this.reliabilitySimulator.GetStatefulServiceContext();
        }

        private IStateProvider2 CreateStateProvider(Uri name, Type type)
        {
            return (IStateProvider2) Activator.CreateInstance(type);
        }

        private void SetUpReplicaForReads()
        {
            this.reliabilitySimulator.GrantReadAccess();
        }

        private TransactionalReplicator Replicator
        {
            get { return this.reliabilitySimulator.GetTransactionalReplicator(); }
            set { throw new InvalidOperationException("BackupParserImpl.Replicator can't be set from outside of ReliabilitySimulator."); }
        }

        private string backupChainPath;
        private CodePackageInfo codePackage;
        private ReliabilitySimulator reliabilitySimulator;
        private StateManager stateManager;
        private string workFolder;
        private bool seenFirstTransaction; // maintains state to see if we have seen first Transaction or not.
        private TransactionChangeManager transactionChangeManager;
    }
}
