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
using Microsoft.ServiceFabric.Tools.RCBackupParserTypes.UserType;

namespace Microsoft.ServiceFabric.Tools.RCBackupGenerator
{
    class ComplexTypesBackupGenerator
    {
        public async Task GenerateUserData(string logFolder)
        {
            this.logFolder = logFolder;
            await GenerateWithValueType<User>(logFolder, new User());
        }

        internal async Task GenerateWithValueType<ValueType>(string logFolder, ValueType value)
        {
            const int NumberOfStateProviders = 6;

            if (FabricDirectory.Exists(logFolder))
            {
                FabricDirectory.Delete(logFolder, true);
            }

            FabricDirectory.CreateDirectory(logFolder);

            var rand = new Random();

            var reliabilitySimulator = new ReliabilitySimulator.ReliabilitySimulator(
                logFolder,
                new Uri("fabric:/unittest/service" + rand.Next()),
                OnDataLossCallback, // we are never calling OnDataLossAsync on this ReliabilitySimulator.
                CreateStateProvider);

            reliabilitySimulator.CreateReplica(true, false);

            var replicator = reliabilitySimulator.GetTransactionalReplicator();

            // Constants
            var distributedDictionary = new DistributedDictionary<long, ValueType>();
            var distributedQueue = new DistributedQueue<ValueType>();
            var concurrentQueue = new ReliableConcurrentQueue<ValueType>();

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

            await PopulateDictionaryAsync(replicator, DictionaryName, 0, 8, 8, value).ConfigureAwait(false);

            // Take backup
            await replicator.BackupAsync(ComplexDataBackupCallbackAsync).ConfigureAwait(false);

            // Clean up.
            await reliabilitySimulator.DropReplicaAsync();
        }

        internal static async Task PopulateDictionaryAsync<ValueType>(
            TransactionalReplicator transactionalReplicator,
            Uri dictionaryName,
            long startingNumber,
            int batchCount,
            int batchSize,
            ValueType value)
        {
            var dictionary = transactionalReplicator.TryGetStateProvider(dictionaryName).Value as IReliableDictionary<long, ValueType>;
            var distributedDictionary = dictionary as IDistributedDictionary<long, ValueType>;

            var startingCount = distributedDictionary.Count;

            for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                using (var txn = transactionalReplicator.CreateTransaction())
                {
                    for (var key = (batchIndex * batchSize) + startingNumber; key < ((batchIndex + 1) * batchSize) + startingNumber; key++)
                    {
                        await dictionary.AddAsync(txn, key, value).ConfigureAwait(false);
                    }

                    Assert.AreEqual(startingCount + (batchIndex * batchSize), distributedDictionary.Count);
                    await txn.CommitAsync().ConfigureAwait(false);
                }

                Assert.AreEqual(startingCount + ((batchIndex + 1) * batchSize), distributedDictionary.Count);
            }

            var finalCount = distributedDictionary.Count;
            Assert.AreEqual(startingCount + (batchCount * batchSize), finalCount);
        }

        internal Task<bool> ComplexDataBackupCallbackAsync(Data.BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            Assert.IsFalse(cancellationToken.IsCancellationRequested);
            var complexDataBackupFolderPath = Path.Combine(logFolder, "BackupContainer", Guid.NewGuid().ToString("N"));
            FabricDirectory.Copy(backupInfo.Directory, complexDataBackupFolderPath, false);
            return Task.FromResult(true);
        }

        internal static async Task<bool> OnDataLossCallback(CancellationToken token)
        {
            return await Task.FromResult(false);
        }

        internal static IStateProvider2 CreateStateProvider(Uri name, Type type)
        {
            return (IStateProvider2)Activator.CreateInstance(type);
        }

        string logFolder;
        internal static readonly Uri DictionaryName = new Uri("urn:testDictionary");
        internal static readonly Uri QueueName = new Uri("urn:testQueue");
        internal static readonly Uri ConcurrentQueueName = new Uri("urn:testConcurrentQueue");
    }
}
