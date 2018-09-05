using System;
using System.Collections.Generic;
using System.IO;
using System.Fabric.Common;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.Tests
{
    [TestClass]
    public class BackupAfterParseTests : RCBackupParserTestBase
    {
        [ClassInitialize]
        public static async Task BackupAfterParseTestsClassInitialize(TestContext testContext)
        {
            await RCBackupParserTestBase.ClassInitialize(testContext);
        }

        [ClassCleanup]
        public static void BackupAfterParseTestsClassCleanup()
        {
            RCBackupParserTestBase.ClassCleanup();
        }

        [TestMethod]
        public async Task RCBackupParser_TakeBackupAfterParseFinish()
        {
            long initialCountKeysInDict = 0;
            // read a back and then write more keys and take a full back.
            using (var backupParser = new RCBackupParser.RCBackupParser(BackupFolderPath, ""))
            {
                await backupParser.ParseAsync(CancellationToken.None);
                var stateManager = backupParser.StateManager;
                var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);
                Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                var dictionary = result.Value;
                using (var tx = stateManager.CreateTransaction())
                {
                    initialCountKeysInDict = await dictionary.GetCountAsync(tx);
                    await tx.CommitAsync();
                }

                Assert.IsTrue(initialCountKeysInDict > 0, "No data seen in dictionary");

                // Take backup
                await backupParser.BackupAsync(Data.BackupOption.Full, TimeSpan.FromMinutes(30), CancellationToken.None, this.OnFullBackupCallback);
            }

            // Verify full backup
            using (var backupParser = new RCBackupParser.RCBackupParser(this.FullBackupFolderPath, ""))
            {
                await backupParser.ParseAsync(CancellationToken.None);

                var stateManager = backupParser.StateManager;
                var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);
                Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                var dictionary = result.Value;
                using (var tx = stateManager.CreateTransaction())
                {
                    var countKeysInDict = await dictionary.GetCountAsync(tx);

                    Assert.AreEqual(initialCountKeysInDict, countKeysInDict, "Fullbackup does not have right number of keys.");

                    for (long i = 0; i < initialCountKeysInDict; ++i)
                    {
                        var valueResult = await dictionary.TryGetValueAsync(tx, i);
                        Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                        Assert.AreEqual(i, valueResult.Value, "Not able to get expected value");
                    }
                    await tx.CommitAsync();
                }
            }
        }

        [TestMethod]
        public async Task RCBackupParser_TakeBackupAfterWritingToRCs()
        {
            long initialCountKeysInDict = 0, numKeysToAdd = 100;

            // read a back and then write more keys and take a full back.
            using (var backupParser = new RCBackupParser.RCBackupParser(BackupFolderPath, ""))
            {
                await backupParser.ParseAsync(CancellationToken.None);

                var stateManager = backupParser.StateManager;
                var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);
                Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                var dictionary = result.Value;

                using (var tx = stateManager.CreateTransaction())
                {
                    initialCountKeysInDict = await dictionary.GetCountAsync(tx);
                    for (long i = initialCountKeysInDict; i < initialCountKeysInDict + numKeysToAdd; ++i)
                    {
                        await dictionary.AddOrUpdateAsync(tx, i, i + 1, (k, v) => k + 1);
                    }
                    await tx.CommitAsync();
                }

                Assert.IsTrue(initialCountKeysInDict > 0, "No data seen in dictionary");

                // Take backup
                await backupParser.BackupAsync(Data.BackupOption.Full, TimeSpan.FromMinutes(30), CancellationToken.None, this.OnFullBackupCallback);
            }

            // Verify full backup
            using (var backupParser = new RCBackupParser.RCBackupParser(this.FullBackupFolderPath, ""))
            {
                await backupParser.ParseAsync(CancellationToken.None);

                var stateManager = backupParser.StateManager;
                var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);

                Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                var dictionary = result.Value;
                using (var tx = stateManager.CreateTransaction())
                {
                    var countKeysInDict = await dictionary.GetCountAsync(tx);

                    Assert.AreEqual(initialCountKeysInDict + numKeysToAdd, countKeysInDict, "Fullbackup does not have right number of keys.");

                    for (long i = 0; i < initialCountKeysInDict; ++i)
                    {
                        var valueResult = await dictionary.TryGetValueAsync(tx, i);
                        Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                        Assert.AreEqual(i, valueResult.Value, "Not able to get expected value");
                    }

                    for (long i = initialCountKeysInDict; i < countKeysInDict; ++i)
                    {
                        var valueResult = await dictionary.TryGetValueAsync(tx, i);
                        Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                        Assert.AreEqual(i + 1, valueResult.Value, "Not able to get expected value");
                    }

                    await tx.CommitAsync();
                }
            }
        }

        [TestMethod]
        public async Task RCBackupParser_TakeIncrementatlBackupAfterWritingToRCs()
        {
            long initialCountKeysInDict = 0;
            const long numKeysToAdd = 100, numIncrementalKeysToAdd = 20;

            // read a back and then write more keys and take a full back.
            using (var backupParser = new RCBackupParser.RCBackupParser(BackupFolderPath, ""))
            {
                await backupParser.ParseAsync(CancellationToken.None);

                var stateManager = backupParser.StateManager;
                var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);
                Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                var dictionary = result.Value;

                using (var tx = stateManager.CreateTransaction())
                {
                    initialCountKeysInDict = await dictionary.GetCountAsync(tx);
                    var totalKeys = initialCountKeysInDict + numKeysToAdd;
                    for (long i = initialCountKeysInDict; i < totalKeys; ++i)
                    {
                        await dictionary.AddOrUpdateAsync(tx, i, i + 1, (k, v) => k + 1);
                    }
                    await tx.CommitAsync();
                }

                Assert.IsTrue(initialCountKeysInDict > 0, "No data seen in dictionary");

                // Take backup
                await backupParser.BackupAsync(Data.BackupOption.Full, TimeSpan.FromMinutes(30), CancellationToken.None, this.OnFullBackupCallback);

                using (var tx = stateManager.CreateTransaction())
                {
                    var totalKeys = initialCountKeysInDict + numKeysToAdd + numIncrementalKeysToAdd;
                    for (long i = initialCountKeysInDict + numKeysToAdd; i < totalKeys; ++i)
                    {
                        await dictionary.AddOrUpdateAsync(tx, i, i + 2, (k, v) => k + 2);
                    }
                    await tx.CommitAsync();
                }

                // Take incremental backup
                await backupParser.BackupAsync(BackupOption.Incremental, TimeSpan.FromMinutes(3), CancellationToken.None, this.OnIncrementalBackupCallback);
            }

            // Verify full+incremental backups
            using (var backupParser = new RCBackupParser.RCBackupParser(this.FullAndIncrementalBackupFolderPath, ""))
            {
                await backupParser.ParseAsync(CancellationToken.None);

                var stateManager = backupParser.StateManager;
                var result = await stateManager.TryGetAsync<IReliableDictionary<long, long>>(DictionaryName);

                Assert.IsTrue(result.HasValue, "Not able to find IReliableDictionary<long, long> dictionary");

                var dictionary = result.Value;
                using (var tx = stateManager.CreateTransaction())
                {
                    var countKeysInDict = await dictionary.GetCountAsync(tx);

                    Assert.AreEqual(initialCountKeysInDict + numKeysToAdd + numIncrementalKeysToAdd, countKeysInDict, "Fullbackup does not have right number of keys.");

                    for (long i = 0; i < initialCountKeysInDict; ++i)
                    {
                        var valueResult = await dictionary.TryGetValueAsync(tx, i);
                        Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                        Assert.AreEqual(i, valueResult.Value, "Not able to get expected value");
                    }

                    for (long i = initialCountKeysInDict; i < initialCountKeysInDict + numKeysToAdd; ++i)
                    {
                        var valueResult = await dictionary.TryGetValueAsync(tx, i);
                        Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                        Assert.AreEqual(i + 1, valueResult.Value, "Not able to get expected value");
                    }

                    for (long i = initialCountKeysInDict + numKeysToAdd; i < countKeysInDict; ++i)
                    {
                        var valueResult = await dictionary.TryGetValueAsync(tx, i);
                        Assert.IsTrue(valueResult.HasValue, "Value not present in dictionary");
                        Assert.AreEqual(i + 2, valueResult.Value, "Not able to get expected value");
                    }

                    await tx.CommitAsync();
                }
            }
        }

        private Task<bool> OnIncrementalBackupCallback(BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            Assert.IsFalse(cancellationToken.IsCancellationRequested);
            FullAndIncrementalBackupFolderPath = Path.Combine(ClassTestPath, BackupContainerFolderName, Guid.NewGuid().ToString("N"));
            var incrementalDataBackupPath = Path.Combine(FullAndIncrementalBackupFolderPath, Guid.NewGuid().ToString("N"));
            var fullDataBackupPath = Path.Combine(FullAndIncrementalBackupFolderPath, Guid.NewGuid().ToString("N"));

            FabricDirectory.Copy(backupInfo.Directory, incrementalDataBackupPath, false);
            FabricDirectory.Copy(this.FullBackupFolderPath, fullDataBackupPath, false);
            return Task.FromResult(true);
        }

        private Task<bool> OnFullBackupCallback(BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            Assert.IsFalse(cancellationToken.IsCancellationRequested);
            FullBackupFolderPath = Path.Combine(ClassTestPath, BackupContainerFolderName, Guid.NewGuid().ToString("N"));
            FabricDirectory.Copy(backupInfo.Directory, FullBackupFolderPath, false);
            return Task.FromResult(true);
        }

        private string FullBackupFolderPath { get; set; }
        private string FullAndIncrementalBackupFolderPath { get; set; }
    }
}
