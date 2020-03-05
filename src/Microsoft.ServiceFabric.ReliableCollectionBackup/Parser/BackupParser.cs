// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Fabric;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.ServiceFabric.Data;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// BackupParser parses the backup of Service Fabric stateful service viz. Reliable Collections.
    /// This class can be used to 
    /// 1) Parse a backup chain,
    /// 2) Validate data via notifications,
    /// 3) Make additional changes and take a new backup.
    /// </summary>
    public class BackupParser : IDisposable
    {        
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor for BackupParser.
        /// </summary>
        /// <param name="backupChainPath">Folder path that contains sub folders of one full and multiple incremental backups.</param>
        /// Pass an empty string if code package is not required for backup parsing. e.g. when backup has only primitive types.        
        public BackupParser(string backupChainPath)
        {
            this.backupParserImpl = new BackupParserImpl(backupChainPath, String.Empty);
        }

        /// <summary>
        /// Constructor for BackupParser.
        /// </summary>
        /// <param name="backupChainPath">Folder path that contains sub folders of one full and multiple incremental backups.</param>
        /// <param name="codePackagePath">Code packages of the service whose backups are provided in <paramref name="backupChainPath" />.
        /// Pass an empty string if code package is not required for backup parsing. e.g. when backup has only primitive types.
        /// </param>
        public BackupParser(string backupChainPath, string codePackagePath)
        {
            this.backupParserImpl = new BackupParserImpl(backupChainPath, codePackagePath);
        }

        /// <summary>
        /// Events fired when a transaction is committed.
        /// This event contains the changes that were applied in this transaction.
        /// During this event, user has a consistent view of the backup at this point in time.
        /// We can use StateManager to read (not write) complete Reliable Collections at this time.
        /// </summary>
        public event EventHandler<NotifyTransactionAppliedEventArgs> TransactionApplied
        {
            add
            {
                this.backupParserImpl.TransactionApplied += value;
            }
            remove
            {
                this.backupParserImpl.TransactionApplied -= value;
            }
        }

        /// <summary>
        /// Parses a backup.
        /// Before parsing, one could register for <see cref="TransactionApplied" /> transaction events during parsing. These events are fired when a committed transaction is being parsed.
        /// After parsing has finished, one can write to the Reliable Collections using <see cref="StateManager" />
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Task that represents the asynchronous parse operation.</returns>
        public async Task ParseAsync(CancellationToken cancellationToken)
        {
            log.Info("Parser ParseAsync Call");
            await this.backupParserImpl.ParseAsync(cancellationToken);
        }

        /// <summary>
        /// Takes a backup of the current state of Reliable Collections.
        /// </summary>
        /// <param name="backupOption">The type of backup to perform.</param>
        /// <param name="timeout">The timeout for this operation.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="backupCallback">Callback to be called when the backup folder has been created locally and is ready to be moved out of the node.</param>
        /// <returns>Task that represents the asynchronous backup operation.</returns>
        public async Task BackupAsync(BackupOption backupOption, TimeSpan timeout, CancellationToken cancellationToken, Func<BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            log.Info("Parser BackupAsync Call");
            await this.backupParserImpl.BackupAsync(backupCallback, backupOption, timeout, cancellationToken);
        }

        /// <summary>
        /// Cleans up any resources like folders used by <see cref="BackupParser"/>.
        /// </summary>
        public void Dispose()
        {
            log.Info("Rest Server Dispose Event called.");
            this.backupParserImpl.Dispose();
        }

        /// <summary>
        /// <see cref="IReliableStateManager"/> which is used for reading and writing to the Reliable Collections of the backup.
        /// Writing is only allowed after backup has been fully parsed after <see cref="ParseAsync"/>.
        /// </summary>
        public IReliableStateManager StateManager
        {
            get { return this.backupParserImpl.StateManager; }
            internal set { throw new InvalidOperationException("Setting BackupParser.StateManager is not allowed."); }
        }

        /// <summary>
        /// Gets the stateful service context of the Replica.
        /// </summary>
        /// <returns>StatefulServiceContext associated with Replica of this Parser</returns>
        internal StatefulServiceContext GetStatefulServiceContext()
        {
            return this.backupParserImpl.GetStatefulServiceContext();
        }

        /// <summary>
        /// Actual implementation of <see cref="BackupParser"/>
        /// </summary>
        private BackupParserImpl backupParserImpl;
    }
}
