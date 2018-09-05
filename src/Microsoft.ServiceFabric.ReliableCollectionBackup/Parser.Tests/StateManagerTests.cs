// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.Tests
{
    [TestClass]
    public class StateManagerTests : RCBackupParserTestBase
    {
        [ClassInitialize]
        public static async Task StateManagerTestsClassInitialize(TestContext testContext)
        {
            await RCBackupParserTestBase.ClassInitialize(testContext);
        }

        [ClassCleanup]
        public static void StateManagerTestsClassCleanup()
        {
            RCBackupParserTestBase.ClassCleanup();
        }

        [TestMethod]
        public async Task RCBackupParser_StateManagerAbleToReadInTransactionApplied()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
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
        public async Task RCBackupParser_StateManagerFailsToWriteDuringParse()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
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
        public async Task RCBackupParser_StateManagerAbleToWriteAfterParseFinish()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
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

                // Verify
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
    }
}
