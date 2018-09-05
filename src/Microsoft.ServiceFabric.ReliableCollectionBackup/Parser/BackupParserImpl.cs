using System;
using System.Collections.Generic;
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
    internal class BackupParserImpl : IDisposable
    {
        public BackupParserImpl(string backupChainFolderPath, string codePackagePath)
        {
            this.backupChain = new BackupChainInfo(new List<string>() { backupChainFolderPath });
            this.codePackage = new CodePackageInfo(codePackagePath);
            this.workFolder = Path.Combine(Directory.GetCurrentDirectory(), "RCBackupParser", Guid.NewGuid().ToString());

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

        public event EventHandler<NotifyTransactionAppliedEventArgs> TransactionApplied;

        public async Task ParseAsync(CancellationToken cancellationToken)
        {
            await ComMtaHelper.WrapNativeSyncInvokeInMTA(async () =>
            {
                var transactionalReplicatorSettings = TransactionalReplicatorSettingsHelper.Create(
                    this.workFolder,
                    "Ktl",
                    checkpointThresholdMB: 200, // keep these settings configurable for the user to play for performance.
                    useDefaultSharedLogId: true,
                    LogManagerLoggerType : System.Fabric.Data.Log.LogManager.LoggerType.Inproc);

                Console.WriteLine("Log path : {0}", transactionalReplicatorSettings.PublicSettings.SharedLogPath);

                await this.reliabilitySimulator.OpenReplicaWithSettingsAsync(ReplicaOpenMode.New, transactionalReplicatorSettings).ConfigureAwait(false);
                await this.reliabilitySimulator.PromoteReplicaAsync(false).ConfigureAwait(false);

                await this.reliabilitySimulator.PrepareForDataLossAsync().ConfigureAwait(false);

                this.Replicator.TransactionChanged += this.OnTransactionChanged;
                this.Replicator.StateManager.StateManagerChanged += this.OnStateManagerChanged;

                await this.reliabilitySimulator.OnDataLossAsync(cancellationToken).ConfigureAwait(false);
            },
            "RCBackupParserImpl.ParseAsync");
        }

        public StateManager StateManager
        {
            get { return this.stateManager; }
            internal set { }
        }

        public async Task BackupAsync(Func<BackupInfo, CancellationToken, Task<bool>> backupCallback, BackupOption backupOption, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await this.Replicator.BackupAsync(backupCallback, backupOption, timeout, cancellationToken);
        }

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
                bool isReliableDict = GenericUtils.IsSubClassOfGeneric(reliableStateType, typeof(IReliableDictionary<,>));

                if (isReliableDict) // move to switch
                {
                    var keyType = reliableStateType.GetGenericArguments()[0];
                    var valueType = reliableStateType.GetGenericArguments()[1];

                    this.GetType().GetMethod("AssignDictionaryChangedHandle", BindingFlags.Instance | BindingFlags.NonPublic)
                        .MakeGenericMethod(keyType, valueType)
                        .Invoke(this, new object[] { operation.ReliableState });
                }
            }
        }


        private void AssignDictionaryChangedHandle<TKey, TValue>(IReliableDictionary<TKey, TValue> dictionary)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            dictionary.DictionaryChanged += this.OnDictionaryChanged;
        }

        private void OnDictionaryChanged<TKey, TValue>(object sender, NotifyDictionaryChangedEventArgs<TKey, TValue> e)
        {
            var keyAddArgs = e as NotifyDictionaryItemAddedEventArgs<TKey, TValue>;
            var reliableState = sender as IReliableState;

            if (null == keyAddArgs || null == reliableState)
            {
                // log here.
                return;
            }

            this.transactionChangeManager.CollectChanges(reliableState.Name, e);
        }

        private void OnTransactionChanged(object sender, NotifyTransactionChangedEventArgs e)
        {
            if (e.Action == NotifyTransactionChangedAction.Commit)
            {
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
                    this.transactionChangeManager.ShowChanges());

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
            await this.Replicator.RestoreAsync(this.backupChain.CommonRootFolder, restorePolicy, cancellationToken).ConfigureAwait(false);
            return true;
        }

        public void Dispose()
        {
            this.reliabilitySimulator.DropReplicaAsync().GetAwaiter().GetResult();
            if (Directory.Exists(this.workFolder))
            {
                Directory.Delete(this.workFolder, true);
            }
            this.codePackage.Dispose();
        }

        internal StatefulServiceContext GetStatefulServiceContext()
        {
            return this.reliabilitySimulator.GetStatefulServiceContext();
        }

        private IStateProvider2 CreateStateProvider(Uri name, Type type)
        {
            return (IStateProvider2)Activator.CreateInstance(type);
        }

        private void SetUpReplicaForReads()
        {
            this.reliabilitySimulator.GrantReadAccess();
        }

        private TransactionalReplicator Replicator
        {
            get { return this.reliabilitySimulator.GetTransactionalReplicator(); }
            set { }
        }

        private BackupChainInfo backupChain;
        private CodePackageInfo codePackage;
        private ReliabilitySimulator reliabilitySimulator;
        private StateManager stateManager;
        private string workFolder;
        private bool seenFirstTransaction;
        private TransactionChangeManager transactionChangeManager;
    }
}