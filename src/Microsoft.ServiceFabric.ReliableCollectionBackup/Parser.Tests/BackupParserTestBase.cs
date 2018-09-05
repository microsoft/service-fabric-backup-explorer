// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Fabric;
using System.Fabric.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Collections.ReliableConcurrentQueue;
using Microsoft.ServiceFabric.Replicator;
using Microsoft.ServiceFabric.Tools.ReliabilitySimulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.Tests
{
    [TestClass]
    public class BackupParserTestBase
    {
        public static async Task ClassInitialize(TestContext testContext)
        {
            lock(ClassInitializationTaskWaitObject)
            {
                if (ClassInitializationTask == null)
                {
                    ClassTestPath = Path.Combine(Environment.CurrentDirectory, BaseTestFolderName);
                    ClassInitializationTask = GenerateBackup(Path.Combine(ClassTestPath, Guid.NewGuid().ToString("N")));
                }
            }

            await ClassInitializationTask;
        }

        public static void ClassCleanup()
        {
            if (FabricDirectory.Exists(ClassTestPath))
            {
                FabricDirectory.Delete(ClassTestPath, true);
            }
        }

        internal static IStateProvider2 CreateStateProvider(Uri name, Type type)
        {
            return (IStateProvider2)Activator.CreateInstance(type);
        }

        protected static async Task GenerateBackup(string logFolder)
        {
            const int NumberOfStateProviders = 6;

            if (FabricDirectory.Exists(logFolder))
            {
                FabricDirectory.Delete(logFolder, true);
            }

            FabricDirectory.CreateDirectory(logFolder);

            var reliabilitySimulator = new ReliabilitySimulator(
                logFolder,
                new Uri("fabric:/unittest/service"),
                OnDataLossCallback,
                CreateStateProvider);

            reliabilitySimulator.CreateReplica(true, false);

            var replicator = reliabilitySimulator.GetTransactionalReplicator();

            // Constants
            var distributedDictionary = new DistributedDictionary<long, long>();
            var distributedQueue = new DistributedQueue<long>();
            var concurrentQueue = new ReliableConcurrentQueue<long>();

            // Setup
            await reliabilitySimulator.OpenReplicaAsync(ReplicaOpenMode.New).ConfigureAwait(false);
            await reliabilitySimulator.PromoteReplicaAsync(false).ConfigureAwait(false);

            var result = replicator.CreateAsyncEnumerable(false, false);
            Assert.AreEqual(0, result.ToEnumerable().Count(), "State Manager must be empty");

            // Write data.
            using (var txn = replicator.CreateTransaction())
            {
                await replicator.AddStateProviderAsync(txn, DictionaryName, distributedDictionary).ConfigureAwait(false);
                await replicator.AddStateProviderAsync(txn, QueueName, distributedQueue).ConfigureAwait(false);
                await replicator.AddStateProviderAsync(txn, ConcurrentQueueName, concurrentQueue).ConfigureAwait(false);
                await txn.CommitAsync().ConfigureAwait(false);
            }

            result = replicator.CreateAsyncEnumerable(false, false);
            Assert.AreEqual(NumberOfStateProviders, result.ToEnumerable().Count(), "State Manager must include all the state providers");

            // Assumptions by tests for the backup generated.
            // 1. TotalDictionaryInserts contains total no of keys in dictionary.
            // 2. If there are n keys in dictionary, then they are from 0 - (n-1).

            int batchCount = 8, batchSize = 8;
            await CollectionHelper.PopulateDictionaryAsync(replicator, DictionaryName, 0, batchCount, batchSize).ConfigureAwait(false);
            await CollectionHelper.PopulateQueueAsync(replicator, QueueName, 0, batchCount, batchSize).ConfigureAwait(false);
            await CollectionHelper.PopulateConcurrentQueueAsync(replicator, ConcurrentQueueName, 0, batchCount, batchSize).ConfigureAwait(false);

            // Take backup
            await replicator.BackupAsync(BackupCallbackAsync).ConfigureAwait(false);

            // Clean up.
            await reliabilitySimulator.DropReplicaAsync();

            TotalDictionaryInserts = batchCount * batchSize;
        }

        internal static Task<bool> BackupCallbackAsync(Data.BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            Assert.IsFalse(cancellationToken.IsCancellationRequested);
            BackupFolderPath = Path.Combine(ClassTestPath, BackupContainerFolderName, Guid.NewGuid().ToString("N"));
            FabricDirectory.Copy(backupInfo.Directory, BackupFolderPath, false);
            return Task.FromResult(true);
        }

        internal static async Task<bool> OnDataLossCallback(CancellationToken token)
        {
            return await Task.FromResult(false);
        }

        private static volatile Task ClassInitializationTask = null;
        private static object ClassInitializationTaskWaitObject = new object();
        protected const string BaseTestFolderName = "BackupParserTests";
        protected const string BackupContainerFolderName = "BackupContainer";
        protected static readonly Uri DictionaryName = new Uri("urn:testDictionary");
        protected static readonly Uri QueueName = new Uri("urn:testQueue");
        protected static readonly Uri ConcurrentQueueName = new Uri("urn:testConcurrentQueue");

        protected static string ClassTestPath { get; set; }
        protected static string BackupFolderPath { get; set; }
        protected static int TotalDictionaryInserts { get; set; }
    }
}
