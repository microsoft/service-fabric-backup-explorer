// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using log4net;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System.Reflection;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// Lists different kind of ReliableState that are supported.
    /// </summary>
    internal enum ReliableStateKind
    {
        ReliableDictionary,
        ReliableQueue,
        ReliableConcurrentQueue
    }

    internal static class ReliableStateKindUtils
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static ReliableStateKind KindOfReliableState(IReliableState reliableState)
        {
            var reliableStateType = reliableState.GetType();

            if (GenericUtils.IsSubClassOfGeneric(reliableStateType, typeof(IReliableDictionary<,>)))
            {
                return ReliableStateKind.ReliableDictionary;
            }
            else if (GenericUtils.IsSubClassOfGeneric(reliableStateType, typeof(IReliableQueue<>)))
            {
                return ReliableStateKind.ReliableQueue;
            }
            else if (GenericUtils.IsSubClassOfGeneric(reliableStateType, typeof(IReliableConcurrentQueue<>)))
            {
                return ReliableStateKind.ReliableConcurrentQueue;
            }

            throw new ArgumentException("Type {0} is not supported.", reliableStateType.FullName);
        }
    }
}
