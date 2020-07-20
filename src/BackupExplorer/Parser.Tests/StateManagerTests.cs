// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.Tests
{
    [TestClass]
    public class StateManagerTests : BackupParserTestBase
    {
        [ClassInitialize]
        public static async Task StateManagerTestsClassInitialize(TestContext testContext)
        {
            await BackupParserTestBase.ClassInitialize(testContext);
        }

        [ClassCleanup]
        public static void StateManagerTestsClassCleanup()
        {
            BackupParserTestBase.ClassCleanup();
        }

        [TestMethod]
        public async Task BackupParser_StateManagerAbleToReadInTransactionApplied()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
                backupParser.ReliableStateTypeKnown += (sender, args) =>
                {

                };
                long countValuesInDictionary = 0;

                backupParser.TransactionApplied += async (sender, args) =>
                {
                    var stateManager = backupParser.StateManager;
                    var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);
                    Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                    var dictionary = result.Value;
                    using (var tx = stateManager.CreateTransaction())
                    {
                        var count = await dictionary.GetCountAsync(tx);

                        for (int i = 0; i < count; ++i)
                        {
                            var valueResult = await dictionary.TryGetValueAsync(tx, i);
                            Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                            Assert.AreEqual(i, valueResult.Value, "Not able to get expected value");
                        }

                        countValuesInDictionary = count;

                        await tx.CommitAsync();
                    }
                };

                await backupParser.ParseAsync(CancellationToken.None);
                Assert.IsTrue(countValuesInDictionary > 0, "No data read in dictionary.");
            }
        }

        [TestMethod]
        public async Task BackupParser_StateManagerFailsToWriteDuringParse()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
                backupParser.ReliableStateTypeKnown += (sender, args) =>
                {

                };
                long countValuesInDictionary = 0;

                backupParser.TransactionApplied += async (sender, args) =>
                {
                    var stateManager = backupParser.StateManager;
                    var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);
                    Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                    var dictionary = result.Value;

                    try
                    {
                        using (var tx = stateManager.CreateTransaction())
                        {
                            countValuesInDictionary = await dictionary.GetCountAsync(tx);

                            for (int i = 0; i < countValuesInDictionary; ++i)
                            {
                                var valueResult = await dictionary.TryGetValueAsync(tx, i);
                                Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                                Assert.AreEqual(i, valueResult.Value, "Not able to get expected value");

                                await dictionary.AddOrUpdateAsync(tx, i, i + 1, (k, v) => v + 1);
                            }

                            await tx.CommitAsync();
                        }

                        if (countValuesInDictionary > 0)
                        {
                            // If we called AddOrUpdateAsync, commit should fail.
                            Assert.Fail("This transaction should not have committed.");
                        }
                    }
                    catch (InvalidOperationException)
                    { }
                    catch (FabricNotPrimaryException)
                    { }
                };

                await backupParser.ParseAsync(CancellationToken.None);
                Assert.IsTrue(countValuesInDictionary > 0, "No data read in dictionary.");
            }
        }

        [TestMethod]
        public async Task BackupParser_StateManagerAbleToWriteAfterParseFinish()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
                backupParser.ReliableStateTypeKnown += (sender, args) =>
                {

                };
                bool transactionSeen = false;
                var stateManager = backupParser.StateManager;

                backupParser.TransactionApplied += (sender, args) =>
                {
                    transactionSeen = true;
                };

                await backupParser.ParseAsync(CancellationToken.None);
                Assert.IsTrue(transactionSeen, "No transaction seen.");

                // Make writes..
                {
                    var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);
                    Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                    long countKeysInDict = 0;
                    var dictionary = result.Value;

                    using (var tx = stateManager.CreateTransaction())
                    {
                        countKeysInDict = await dictionary.GetCountAsync(tx);

                        for (int i = 0; i < countKeysInDict; ++i)
                        {
                            var valueResult = await dictionary.TryGetValueAsync(tx, i);
                            Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                            Assert.AreEqual(i, valueResult.Value, "Not able to get expected value");

                            await dictionary.AddOrUpdateAsync(tx, i, i + 1, (k, v) => v + 1);
                        }

                        await tx.CommitAsync();
                    }

                    Assert.IsTrue(countKeysInDict > 0, "No data seen in dictionary");
                }

                // Verify writes
                {
                    var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);
                    Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                    long countKeysInDict = 0;
                    var dictionary = result.Value;

                    using (var tx = stateManager.CreateTransaction())
                    {
                        countKeysInDict = await dictionary.GetCountAsync(tx);

                        for (int i = 0; i < countKeysInDict; ++i)
                        {
                            var valueResult = await dictionary.TryGetValueAsync(tx, i);
                            Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                            Assert.AreEqual(i + 1, valueResult.Value, "Not able to get expected value");
                        }

                        await tx.CommitAsync();
                    }

                    Assert.IsTrue(countKeysInDict > 0, "No data seen in dictionary");
                }
            }
        }

        [TestMethod]
        public async Task BackupParser_StateManagerAbleToReadQueuesInTransactionApplied()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
                backupParser.ReliableStateTypeKnown += (sender, args) =>
                {

                };
                long countValuesInQueue = 0;

                backupParser.TransactionApplied += async (sender, args) =>
                {
                    var stateManager = backupParser.StateManager;
                    // verify ReliableQueue
                    var result = await stateManager.TryGetAsync<IReliableQueue<long>>(QueueName);
                    Assert.IsTrue(result.HasValue, "Not able to find IReliableQueue<long> queue");

                    var queue = result.Value;
                    using (var tx = stateManager.CreateTransaction())
                    {
                        countValuesInQueue = await queue.GetCountAsync(tx);
                        if (countValuesInQueue > 0)
                        {
                            var valueResult = await queue.TryPeekAsync(tx);
                            Assert.IsTrue(valueResult.HasValue, "Value not present in queue");
                            Assert.AreEqual(0, valueResult.Value, "Queue head should be always first element 0");
                        }
                        await tx.CommitAsync();
                    }
                    // Don't verify ConcurrentQueue
                    // There is no read only api in ConcurrentQueue : only enque/deque.
                    // Reading Count on queue does not work because it checks for TransactionalReplicator's IsReadable which is not true.
                };

                await backupParser.ParseAsync(CancellationToken.None);
                Assert.AreEqual(TotalQueueTransactions * NumOperationsPerTransaction, countValuesInQueue, "No data read in queue.");
            }
        }
    }
}
