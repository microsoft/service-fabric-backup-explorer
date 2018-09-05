// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// ReliableCollectionChange stores the changes in a ReliableCollection during a transaction.
    /// </summary>
    public class ReliableCollectionChange
    {
        /// <summary>
        /// Constructor of ReliableCollectionChange
        /// </summary>
        /// <param name="name">Name of Reliable Collection</param>
        public ReliableCollectionChange(Uri name)
        {
            this.Name = name;
            this.Changes = new List<EventArgs>();
        }

        /// <summary>
        /// Name of ReliableState
        /// </summary>
        public Uri Name { get; }

        /// <summary>
        /// Events that are received from this Reliable State.
        /// </summary>
        public List<EventArgs> Changes { get; }
    }
}
