// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Collections.ReliableConcurrentQueue;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.Tests
{
    [TestClass]
    public class GenericsTests
    {
        [TestMethod]
        public void RCBackupParser_IsSubClassOfGenericTrue()
        {
            Assert.IsTrue(GenericUtils.IsSubClassOfGeneric(typeof(DistributedDictionary<long, long>), typeof(IReliableDictionary<,>)));
            Assert.IsTrue(GenericUtils.IsSubClassOfGeneric(new DistributedDictionary<long, long>().GetType(), typeof(IReliableDictionary<,>)));

            Assert.IsTrue(GenericUtils.IsSubClassOfGeneric(typeof(DistributedQueue<long>), typeof(IReliableQueue<>)));
            Assert.IsTrue(GenericUtils.IsSubClassOfGeneric(new DistributedQueue<long>().GetType(), typeof(IReliableQueue<>)));

            Assert.IsTrue(GenericUtils.IsSubClassOfGeneric(typeof(ReliableConcurrentQueue<long>), typeof(IReliableConcurrentQueue<>)));
            Assert.IsTrue(GenericUtils.IsSubClassOfGeneric(new ReliableConcurrentQueue<long>().GetType(), typeof(IReliableConcurrentQueue<>)));
        }

        [TestMethod]
        public void RCBackupParser_IsSubClassOfGenericFalse()
        {
            Assert.IsFalse(GenericUtils.IsSubClassOfGeneric(typeof(IReliableConcurrentQueue<long>), typeof(IReliableDictionary<,>)));
            Assert.IsFalse(GenericUtils.IsSubClassOfGeneric(new ReliableConcurrentQueue<long>().GetType(), typeof(IReliableDictionary<,>)));

            Assert.IsFalse(GenericUtils.IsSubClassOfGeneric(typeof(DistributedQueue<long>), typeof(IReliableDictionary<,>)));
            Assert.IsFalse(GenericUtils.IsSubClassOfGeneric(new DistributedQueue<long>().GetType(), typeof(IReliableDictionary<,>)));

            Assert.IsFalse(GenericUtils.IsSubClassOfGeneric(typeof(DistributedQueue<long>), typeof(IReliableConcurrentQueue<>)));
            Assert.IsFalse(GenericUtils.IsSubClassOfGeneric(new DistributedQueue<long>().GetType(), typeof(IReliableConcurrentQueue<>)));

            Assert.IsFalse(GenericUtils.IsSubClassOfGeneric(typeof(DistributedDictionary<long, long>), typeof(IReliableQueue<>)));
            Assert.IsFalse(GenericUtils.IsSubClassOfGeneric(new DistributedDictionary<long, long>().GetType(), typeof(IReliableQueue<>)));
        }
    }
}
