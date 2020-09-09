// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Data.Collections.ReliableConcurrentQueue;
    using Microsoft.ServiceFabric.Replicator;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class CollectionHelper
    {
        internal static IReliableDictionary<TKey, TValue> GetDictionary<TKey, TValue>(TransactionalReplicator transactionalReplicator, Uri dictionaryName)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var result = transactionalReplicator.TryGetStateProvider(dictionaryName);
            Assert.IsTrue(result.HasValue, "State Provider must exist");
            var dictionary = result.Value as IReliableDictionary<TKey, TValue>;
            Assert.IsNotNull(dictionary, "State provider named '{0}' is not an IDistributedDictionary<long, long>", dictionaryName);

            return dictionary;
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

        internal static IReliableConcurrentQueue<T> GetConcurrentQueue<T>(TransactionalReplicator transactionalReplicator, Uri concurrentQueueName)
        {
            var result = transactionalReplicator.TryGetStateProvider(concurrentQueueName);

            Assert.IsTrue(result.HasValue, "State Provider must exist");

            var queue = result.Value as IReliableConcurrentQueue<T>;

            Assert.IsNotNull(queue, "State provider named '{0}' is not an IReliableConcurrentQueue<T>", concurrentQueueName);

            return queue;
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

        public static void VerifyConcurrentQueueUnordered(IReliableConcurrentQueue<long> queue, IEnumerable<long> expectedValues, string message = "")
        {
            var expected = expectedValues.ToList();
            expected.Sort((val1, val2) => val1.CompareTo(val2));
            Assert.AreEqual(expected.Count, queue.Count, message);

            var values = ((ReliableConcurrentQueue<long>)queue).GetVisibleListElementsUnsafe().ToList();
            values.Sort((val1, val2) => val1.CompareTo(val2));
            Assert.IsTrue(expected.SequenceEqual(values));
        }
    }
}
