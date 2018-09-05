// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Data;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// RCBackupParser is Service Fabric Reliable Collection's backup parser.
    /// </summary>
    public class BackupParser : IDisposable
    {
        /// <summary>
        /// Constructor for RCBackupParser
        /// </summary>
        /// <param name="backupChainFolderPath">Folder path that contains sub folders of full and incremental backups</param>
        /// <param name="codePackagePath">Code packages of the service whose backups are provided in first param</param>
        public BackupParser(string backupChainFolderPath, string codePackagePath)
        {
            this.rcBackupParserImpl = new BackupParserImpl(backupChainFolderPath, codePackagePath);
        }

        /// <summary>
        /// Events fired when a transaction is committed.
        /// This event contains the changes that were applied in this transaction.
        /// With in this event, we have a consistent view of the backup at this point in time.
        /// We can use StateManager to read complete Reliable Collections at this time.
        /// </summary>
        public event EventHandler<NotifyTransactionAppliedEventArgs> TransactionApplied
        {
            add
            {
                this.rcBackupParserImpl.TransactionApplied += value;
            }
            remove
            {
                this.rcBackupParserImpl.TransactionApplied -= value;
            }
        }

        /// <summary>
        /// Parses a backup.
        /// During parsing, transactionApplied events are fired when a transaction is committed.
        /// After parsing has finished, we can write to the Reliable Collections using StateManager.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns></returns>
        public async Task ParseAsync(CancellationToken cancellationToken)
        {
            await this.rcBackupParserImpl.ParseAsync(cancellationToken);
        }

        /// <summary>
        /// Takes a backup of the current state of Reliable Collections.
        /// </summary>
        /// <param name="backupOption">The type of backup to perform.</param>
        /// <param name="timeout">The timeout for this operation.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="backupCallback">Callback to be called when the backup folder has been created locally and is ready to be moved out of the node.</param>
        /// <returns></returns>
        public async Task BackupAsync(BackupOption backupOption, TimeSpan timeout, CancellationToken cancellationToken, Func<BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            await this.rcBackupParserImpl.BackupAsync(backupCallback, backupOption, timeout, cancellationToken);
        }

        /// <summary>
        /// Dispose the RCBackupParser.
        /// </summary>
        public void Dispose()
        {
            this.rcBackupParserImpl.Dispose();
        }

        /// <summary>
        /// StateManager which is used for reading and writing to the Reliable Collections of the backup.
        /// Writing is only allowed after backup has been fully parsed after ParseAsync.
        /// </summary>
        public IReliableStateManager StateManager
        {
            get { return this.rcBackupParserImpl.StateManager; }
            internal set { }
        }

        internal StatefulServiceContext GetStatefulServiceContext()
        {
            return rcBackupParserImpl.GetStatefulServiceContext();
        }

        private BackupParserImpl rcBackupParserImpl;
    }
}
