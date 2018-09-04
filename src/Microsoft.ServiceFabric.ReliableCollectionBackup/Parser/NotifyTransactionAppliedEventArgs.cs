using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Tools.RCBackupParser
{
    /// <summary>
    /// NotifyTransactionAppliedEventArgs is used for notifying TransactionApplied event.
    /// </summary>
    public class NotifyTransactionAppliedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor of NotifyTransactionAppliedEventArgs.
        /// </summary>
        public NotifyTransactionAppliedEventArgs(ITransaction transaction, IEnumerable<ReliableCollectionChange> changes)
        {
            this.TransactionId = transaction.TransactionId;
            this.CommitSequenceNumber = transaction.CommitSequenceNumber;
            this.Changes = changes;
        }

        /// <summary>
        /// Gets a value identifying the transaction.
        /// </summary>
        /// <returns>The transaction id.</returns>
        public long TransactionId { get; }

        /// <summary>
        /// Sequence number for the commit operation.
        /// </summary>
        /// <value>
        /// The sequence number at which the the transaction was committed.
        /// </value>
        public long CommitSequenceNumber { get; }

        /// <summary>
        /// List of reliable collection changes that were made during this transaction.
        /// </summary>
        public IEnumerable<ReliableCollectionChange> Changes { get; }
    }
}
