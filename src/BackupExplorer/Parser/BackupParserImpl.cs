// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Fabric;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using Microsoft.ServiceFabric.Tools.ReliabilitySimulator;
using Microsoft.ServiceFabric.Replicator;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// Actual implementation of BackupParser.
    /// </summary>
    internal class BackupParserImpl : IDisposable
    {
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Constructor for BackupParserImpl.
        /// </summary>
        /// <param name="backupChainPath">Folder path that contains sub folders of one full and multiple incremental backups.</param>
        /// <param name="codePackagePath">Code packages of the service whose backups are provided in <paramref name="backupChainPath" />.</param>
        public BackupParserImpl(string backupChainPath, string codePackagePath, ILog log, string workFolderPath, int checkpointThresholdInMB = 50, string logFolderPath = null)
        {
            this.backupChainPath = backupChainPath;
            this.codePackage = new CodePackageInfo(codePackagePath);
            this.checkpointThresholdInMB = checkpointThresholdInMB;

            if (log != null) BackupParserImpl.log = log;

            if (String.IsNullOrEmpty(workFolderPath))
            {
                this.workFolder = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString()); 
            }
            else
            {
                this.workFolder = Path.Combine(workFolderPath, Guid.NewGuid().ToString());
            }

            this.logFolder = logFolderPath ?? this.workFolder;

            Console.WriteLine("Work Folder : {0}", this.workFolder);
            InitializeBackupParser();
        }

        private void InitializeBackupParser()
        {
            Directory.CreateDirectory(this.workFolder);
            if (!Directory.Exists(this.logFolder))
            {
                Directory.CreateDirectory(this.logFolder);
            }

            this.reliabilitySimulator = new ReliabilitySimulator(
                this.workFolder,
                new Uri("fabric:/rcbackupapp/rcbackupservice"),
                DateTime.UtcNow.ToFileTimeUtc(),
                DateTime.UtcNow.ToFileTimeUtc(),
                this.OnDataLossCallback,
                this.CreateStateProvider);
            this.reliabilitySimulator.CreateReplica(true, false);

            this.stateManager = new StateManager(reliabilitySimulator);
            this.seenFirstTransaction = false;
            this.transactionChangeManager = new TransactionChangeManager(this.reliabilitySimulator);
        }

        /// <summary>
        /// Event that notifies about the committed transactions.
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
                    this.logFolder,
                    "Ktl",
                    checkpointThresholdMB: this.checkpointThresholdInMB,
                    useDefaultSharedLogId: true,
                    LogManagerLoggerType : System.Fabric.Data.Log.LogManager.LoggerType.Inproc);

                await this.reliabilitySimulator.OpenReplicaWithSettingsAsync(ReplicaOpenMode.New, transactionalReplicatorSettings).ConfigureAwait(false);
                await this.reliabilitySimulator.PromoteReplicaAsync(false).ConfigureAwait(false);
                await this.reliabilitySimulator.PrepareForDataLossAsync().ConfigureAwait(false);

                this.Replicator.TransactionChanged += this.OnTransactionChanged;
                this.Replicator.StateManager.StateManagerChanged += this.transactionChangeManager.OnStateManagerChanged;
                this.stateManager.ReAddStateSerializers();

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

        private void OnTransactionChanged(object sender, NotifyTransactionChangedEventArgs e)
        {
            log.Info($"Transaction Changed Event Called for Id: {e.Transaction.TransactionId}");
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
            catch (Exception ex)
            {
                log.Info(ex);
                Console.WriteLine($"Exception in Transaction {e.Transaction.TransactionId} \n {ex} ");
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
            try
            {
                if (Directory.Exists(this.workFolder))
                {
                    Directory.Delete(this.workFolder, true);
                }
            } catch (DirectoryNotFoundException e)
            {
                //Directory to be deleted has already been deleted
                log.Info("Could not find directory "+ workFolder + " to delete. Exception: " + e.Message);
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
        private string logFolder;
        private bool seenFirstTransaction; // maintains state to see if we have seen first Transaction or not.
        private TransactionChangeManager transactionChangeManager;
        private int checkpointThresholdInMB;
    }
}
