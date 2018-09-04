// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Tools.RCBackupParserTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Data.Collections.ReliableConcurrentQueue;
    using Microsoft.ServiceFabric.Replicator;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class CollectionHelper
    {
        public static async Task<IReliableDictionary<TKey, TValue>> CreateDictionaryAsync<TKey, TValue>(
            TransactionalReplicator transactionalReplicator,
            Uri dictionaryName) where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            using (var txn = transactionalReplicator.CreateTransaction())
            {
                var distributedDictionary = new DistributedDictionary<TKey, TValue>();
                await transactionalReplicator.AddStateProviderAsync(txn, dictionaryName, distributedDictionary).ConfigureAwait(false);
                await txn.CommitAsync().ConfigureAwait(false);
            }

            return GetDictionary<TKey, TValue>(transactionalReplicator, dictionaryName);
        }

        public static async Task RemoveDictionaryAsync(TransactionalReplicator transactionalReplicator, Uri dictionaryName)
        {
            using (var txn = transactionalReplicator.CreateTransaction())
            {
                await transactionalReplicator.RemoveStateProviderAsync(txn, dictionaryName).ConfigureAwait(false);
                await txn.CommitAsync().ConfigureAwait(false);
            }
        }

        public static async Task PopulateDictionaryAsync(
            TransactionalReplicator transactionalReplicator,
            Uri dictionaryName,
            long startingNumber,
            int batchCount,
            int batchSize)
        {
            var dictionary = GetDictionary<long, long>(transactionalReplicator, dictionaryName);
            IDistributedDictionary<long, long> distributedDictionary = dictionary as IDistributedDictionary<long, long>;

            var startingCount = distributedDictionary.Count;

            for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                using (var txn = transactionalReplicator.CreateTransaction())
                {
                    for (var key = (batchIndex * batchSize) + startingNumber; key < ((batchIndex + 1) * batchSize) + startingNumber; key++)
                    {
                        await dictionary.AddAsync(txn, key, key).ConfigureAwait(false);
                    }

                    Assert.AreEqual(startingCount + (batchIndex * batchSize), distributedDictionary.Count);
                    await txn.CommitAsync().ConfigureAwait(false);
                }

                Assert.AreEqual(startingCount + ((batchIndex + 1) * batchSize), distributedDictionary.Count);
            }

            var finalCount = distributedDictionary.Count;
            Assert.AreEqual(startingCount + (batchCount * batchSize), finalCount);

            await VerifyDictionaryAsync(transactionalReplicator, dictionaryName, startingNumber, batchCount, batchSize).ConfigureAwait(false);
        }

        public static async Task VerifyDictionaryAsync(
            TransactionalReplicator transactionalReplicator,
            Uri dictionaryName,
            long startingNumber,
            int batchCount,
            int batchSize)
        {
            var dictionary = GetDictionary<long, long>(transactionalReplicator, dictionaryName);

            for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                using (var txn = transactionalReplicator.CreateTransaction())
                {
                    for (var key = (batchIndex * batchSize) + startingNumber; key < (batchIndex + 1) * batchSize; key++)
                    {
                        var value = await dictionary.TryGetValueAsync(txn, key).ConfigureAwait(false);
                        Assert.IsTrue(value.HasValue, "Dictionary must contain {0}", key);
                        Assert.AreEqual(key, value.Value, "Dictionary must contain {0}", key);
                    }

                    await txn.CommitAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task PopulateQueueAsync(
            TransactionalReplicator transactionalReplicator,
            Uri queueName,
            long startingNumber,
            int batchCount,
            int batchSize)
        {
            var queue = GetQueue<long>(transactionalReplicator, queueName);

            for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                using (var txn = transactionalReplicator.CreateTransaction())
                {
                    for (var key = (batchIndex * batchSize) + startingNumber; key < (batchIndex + 1) * batchSize; key++)
                    {
                        await queue.EnqueueAsync(txn, key).ConfigureAwait(false);
                    }

                    await txn.CommitAsync().ConfigureAwait(false);
                }
            }

            await VerifyQueueAsync(transactionalReplicator, queueName, startingNumber, batchCount, batchSize).ConfigureAwait(false);
        }

        public static async Task PopulateConcurrentQueueAsync(
            TransactionalReplicator transactionalReplicator,
            Uri queueName,
            long startingNumber,
            int batchCount,
            int batchSize)
        {
            var queue = GetConcurrentQueue<long>(transactionalReplicator, queueName);
            var expectedValues = new List<long>(batchCount * batchSize);

            for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                using (var txn = transactionalReplicator.CreateTransaction())
                {
                    for (var key = (batchIndex * batchSize) + startingNumber; key < (batchIndex + 1) * batchSize; key++)
                    {
                        await queue.EnqueueAsync(txn, key);
                        expectedValues.Add(key);
                    }

                    await txn.CommitAsync();
                }
            }

            VerifyConcurrentQueueUnordered(queue, expectedValues);
        }

        public static async Task DrainQueueAsync(TransactionalReplicator transactionalReplicator, Uri queueName)
        {
            var queue = GetQueue<long>(transactionalReplicator, queueName);

            using (var txn = transactionalReplicator.CreateTransaction())
            {
                while (true)
                {
                    var result = await queue.TryDequeueAsync(txn).ConfigureAwait(false);

                    if (result.HasValue == false)
                    {
                        await txn.CommitAsync().ConfigureAwait(false);
                        break;
                    }
                }
            }

            using (var tx = transactionalReplicator.CreateTransaction())
            {
                var tmpEnum =
                    await
                    (await queue.CreateEnumerableAsync(tx).ConfigureAwait(false)).GetAsyncEnumerator()
                        .MoveNextAsync(CancellationToken.None)
                        .ConfigureAwait(false);
                Assert.AreEqual(false, tmpEnum, "Queue must be drained");
            }
        }

        public static async Task DrainConcurrentQueueAsync(TransactionalReplicator transactionalReplicator, Uri queueName)
        {
            var queue = GetConcurrentQueue<long>(transactionalReplicator, queueName);

            using (var txn = transactionalReplicator.CreateTransaction())
            {
                ConditionalValue<long> ret;

                do
                {
                    ret = await queue.TryDequeueAsync(txn, CancellationToken.None, TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
                }
                while (ret.HasValue);

                await txn.CommitAsync().ConfigureAwait(false);
            }

            Assert.AreEqual(0, queue.Count, "Queue must be drained");
        }

        public static async Task VerifyQueueAsync(
            TransactionalReplicator transactionalReplicator,
            Uri queueName,
            long startingNumber,
            int batchCount,
            int batchSize)
        {
            var queue = GetQueue<long>(transactionalReplicator, queueName);

            using (var tx = transactionalReplicator.CreateTransaction())
            {
                var enumerableQueue = await queue.CreateEnumerableAsync(tx).ConfigureAwait(false);
                var enumerator = enumerableQueue.GetAsyncEnumerator();
                var count = 0;
                while (await enumerator.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
                {
                    count += 1;
                }

                Assert.AreEqual(batchCount * batchSize, count, "Queue does not have the expected size");
                Assert.AreEqual(
                    batchCount * batchSize,
                    await queue.GetCountAsync(tx).ConfigureAwait(false),
                    "Queue does not have the expected size");
            }

            for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                using (var txn = transactionalReplicator.CreateTransaction())
                {
                    for (var key = (batchIndex * batchSize) + startingNumber; key < (batchIndex + 1) * batchSize; key++)
                    {
                        var enumerable = await queue.CreateEnumerableAsync(txn);
                        var enumerator = enumerable.GetAsyncEnumerator();

                        var foundKey = false;
                        while (await enumerator.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
                        {
                            if (enumerator.Current == key)
                            {
                                foundKey = true;
                                break;
                            }
                        }

                        Assert.IsTrue(foundKey == true, "Queue does not contain {0}", key);
                    }

                    await txn.CommitAsync().ConfigureAwait(false);
                }
            }
        }

        public static void VerifyConcurrentQueueUnordered(
            TransactionalReplicator transactionalReplicator,
            Uri concurrentQueueName,
            long startingNumber,
            int batchCount,
            int batchSize)
        {
            var queue = GetConcurrentQueue<long>(transactionalReplicator, concurrentQueueName);
            Assert.AreEqual(batchCount * batchSize, queue.Count, "Queue does not have the expected size");

            List<long> expectedValues = new List<long>(batchCount * batchSize);
            for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                for (var key = (batchIndex * batchSize) + startingNumber; key < (batchIndex + 1) * batchSize; key++)
                {
                    expectedValues.Add(key);
                }
            }

            VerifyConcurrentQueueUnordered(queue, expectedValues);
        }

        public static void VerifyConcurrentQueue(IReliableConcurrentQueue<long> queue, IList<long> expectedValues, string message = "")
        {
            Assert.AreEqual(expectedValues.Count, queue.Count, message);
            Assert.IsTrue(((ReliableConcurrentQueue<long>)queue).GetVisibleListElementsUnsafe().SequenceEqual(expectedValues), message);
        }

        public static void VerifyConcurrentQueueUnordered(IReliableConcurrentQueue<long> queue, IEnumerable<long> expectedValues, string message = "")
        {
            var expected = expectedValues.ToList();
            expected.Sort((val1, val2) => val1.CompareTo(val2));
            Assert.AreEqual(expected.Count, queue.Count, message);

            var values = ((ReliableConcurrentQueue<long>)queue).GetVisibleListElementsUnsafe().ToList();
            values.Sort((val1, val2) => val1.CompareTo(val2));
            Assert.IsTrue(expected.SequenceEqual(values));
        }

        public static void VerifyDictionarySnapshotContainer(IReliableDictionary<long, long> dictionary, int expectedVisibilitySequenceNumber, bool shouldRetry)
        {
            var tmpDictionary = dictionary as DistributedDictionary<long, long>;
            Assert.IsNotNull(tmpDictionary);
            var maxRetry = shouldRetry ? 8 : 1;

            for (var i = 0; i < maxRetry; i++)
            {
                var currentVisibilitySequenceNumberCount = tmpDictionary.DataStore.SnapshotContainer.GetCount();

                if (currentVisibilitySequenceNumberCount == expectedVisibilitySequenceNumber)
                {
                    return;
                }
            }

            Assert.Fail();
        }

        internal static IReliableDictionary<TKey, TValue> GetDictionary<TKey, TValue>(TransactionalReplicator transactionalReplicator, Uri dictionaryName)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var result = transactionalReplicator.TryGetStateProvider(dictionaryName);
            Assert.IsTrue(result.HasValue, "State Provider must exist");
            var dictionary = result.Value as IReliableDictionary<TKey, TValue>;
            Assert.IsNotNull(dictionary, "State provider named '{0}' is not an IDistributedDictionary<long, long>", dictionaryName);

            return dictionary;
        }

        internal static async Task PopulateDictionaryAsync(IReliableDictionary<long, long> dictionary, Transaction txn, long startingNumber, int count)
        {
            for (var key = startingNumber; key < (count + startingNumber); key++)
            {
                await dictionary.AddAsync(txn, key, key).ConfigureAwait(false);
            }
        }

        internal static async Task VerifyDictionaryEnumerationAsync(
            IReliableDictionary<long, long> dictionary,
            long startingNumber,
            int count,
            Transaction txn)
        {
            Assert.IsNotNull(txn);
            var enumeration = await dictionary.CreateEnumerableAsync(txn).ConfigureAwait(false);
            await VerifyDictionaryEnumerationAsync(enumeration, startingNumber, count).ConfigureAwait(false);
        }

        internal static async Task VerifyDictionaryEnumerationAsync(
            IAsyncEnumerable<KeyValuePair<long, long>> enumeration,
            long startingNumber,
            int count)
        {
            var enumerator = enumeration.GetAsyncEnumerator();
            var enumCount = 0;
            while (await enumerator.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
            {
                enumCount += 1;
                Assert.IsTrue(enumerator.Current.Key >= startingNumber);
                Assert.IsTrue(enumerator.Current.Key < startingNumber + count);
                Assert.AreEqual(enumerator.Current.Key, enumerator.Current.Value);
            }

            Assert.AreEqual(count, enumCount);
        }

        internal static async Task VerifyDictionaryKeyEnumerationAsync(
            Transaction txn,
            IReliableDictionary2<long, long> dictionary,
            long startingNumber,
            int count,
            bool isOrdered)
        {
            Assert.IsNotNull(txn);

            var enumerationMode = isOrdered ? EnumerationMode.Ordered : EnumerationMode.Unordered;
            var enumeration = await dictionary.CreateKeyEnumerableAsync(txn, enumerationMode).ConfigureAwait(false);

            await VerifyDictionaryKeyEnumerationAsync(enumeration, startingNumber, count, isOrdered).ConfigureAwait(false);
        }

        internal static async Task VerifyDictionaryKeyEnumerationAsync(
            IAsyncEnumerable<long> enumeration,
            long startingNumber,
            int count,
            bool isOrdered)
        {
            var enumerator = enumeration.GetAsyncEnumerator();

            var enumCount = 0;
            var currentKey = startingNumber - 1;
            while (await enumerator.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
            {
                enumCount += 1;

                Assert.IsTrue(enumerator.Current >= startingNumber);
                Assert.IsTrue(enumerator.Current < startingNumber + count);

                if (!isOrdered)
                {
                    continue;
                }

                currentKey++;
                Assert.AreEqual(enumerator.Current, currentKey);
            }

            Assert.AreEqual(count, enumCount);
        }

        internal static IReliableQueue<T> GetQueue<T>(TransactionalReplicator transactionalReplicator, Uri queueName)
        {
            var result = transactionalReplicator.TryGetStateProvider(queueName);
            Assert.IsTrue(result.HasValue, "State Provider must exist");
            var queue = result.Value as IReliableQueue<T>;
            Assert.IsNotNull(
                queue,
                "State provider named '{0}' is not an IDistributedDictionary<long, long>",
                queueName);

            return queue;
        }

        internal static Task VerifyCountAsync<TKey, TValue>(
            TransactionalReplicator transactionalReplicator,
            IReliableDictionary<TKey, TValue> dictionary,
            long expectedCount = -1) where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var distributedDictionary = dictionary as IDistributedDictionary<TKey, TValue>;
            Assert.IsNotNull(distributedDictionary);

            return VerifyCountAsync(transactionalReplicator, distributedDictionary, expectedCount);
        }

        internal static async Task VerifyCountAsync<TKey, TValue>(
            TransactionalReplicator transactionalReplicator,
            IDistributedDictionary<TKey, TValue> dictionary,
            long expectedCount = -1)
        {
            Assert.IsNotNull(dictionary);
            Assert.IsTrue(expectedCount >= -1);

            long txnCount;
            using (var txn = transactionalReplicator.CreateTransaction())
            {
                txnCount = await dictionary.GetCountAsync(txn).ConfigureAwait(false);
            }

            Assert.AreEqual(txnCount, dictionary.Count, "Transactional count and best effort count do not match.");

            if (expectedCount != -1)
            {
                Assert.AreEqual(expectedCount, dictionary.Count, "Unexpected count.");
            }
        }

        internal static IReliableConcurrentQueue<T> GetConcurrentQueue<T>(TransactionalReplicator transactionalReplicator, Uri concurrentQueueName)
        {
            var result = transactionalReplicator.TryGetStateProvider(concurrentQueueName);

            Assert.IsTrue(result.HasValue, "State Provider must exist");

            var queue = result.Value as IReliableConcurrentQueue<T>;

            Assert.IsNotNull(queue, "State provider named '{0}' is not an IReliableConcurrentQueue<T>", concurrentQueueName);

            return queue;
        }
    }
}