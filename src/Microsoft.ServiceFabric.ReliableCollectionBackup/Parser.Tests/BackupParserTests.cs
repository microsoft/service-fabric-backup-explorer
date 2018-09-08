// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ServiceFabric.Data.Notifications;
using Microsoft.ServiceFabric.ReliableCollectionBackup.UserType;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.Tests
{
    [TestClass]
    public class BackupParserTests : BackupParserTestBase
    {
        [ClassInitialize]
        public static async Task BackupParserTestsClassInitialize(TestContext testContext)
        {
            await BackupParserTestBase.ClassInitialize(testContext);
        }

        [ClassCleanup]
        public static void BackupParserTestsClassCleanup()
        {
            BackupParserTestBase.ClassCleanup();
        }

        [TestMethod]
        public async Task BackupParser_FireTransactionAppliedEvents()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
                bool transactionSeen = false;

                backupParser.TransactionApplied += (sender, args) =>
                {
                    transactionSeen = true;
                };

                await backupParser.ParseAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.IsTrue(transactionSeen, "Not able to parse any transaction from backup chain.");
            }
        }

        [TestMethod]
        public async Task BackupParser_CollectChangesInTransactionAppliedEvents()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
                int totalDictionaryAdds = 0;

                backupParser.TransactionApplied += (sender, args) =>
                {
                    foreach (var reliableStateChange in args.Changes)
                    {
                        if (reliableStateChange.Name == DictionaryName)
                        {
                            foreach (var change in reliableStateChange.Changes)
                            {
                                var addChange = change as NotifyDictionaryItemAddedEventArgs<long, long>;
                                if (null != addChange)
                                {
                                    totalDictionaryAdds++;
                                }
                            }
                        }
                    }
                };

                await backupParser.ParseAsync(CancellationToken.None);

                Assert.AreEqual(totalDictionaryAdds, TotalDictionaryInserts, "Not able to collect all Dictionary change events");
            }
        }

        //[TestMethod]
        //public void BackupParser_NoUserClassInDefaultAppDomain()
        //{
        //    var appDomain = AppDomain.CurrentDomain;
        //    var userAssembly = appDomain.GetAssemblies().Where(assembly => assembly.FullName.Contains("User"));
        //    Assert.IsTrue(userAssembly.Count() == 0, string.Format("User assembly {0} is loaded in default app domain. " +
        //        "Test like BackupParser_AbleToFindAssemblyInCodePackage will pass even if functionality is broken.", userAssembly.FirstOrDefault()));
        //}

        [TestMethod]
        public async Task BackupParser_ComplexTypesWithSerializers()
        {
            var complexDataBackupFolderPath = Path.Combine(ClassTestPath, @"..\UserFullBackup");
            // from this test run's bin\Debug\netstandard2.0\<testname> dir to parent of bin
            var codePackagePath = Path.Combine(ClassTestPath, @"..\..\..\..\UserType\bin\");

            using (var backupParser = new BackupParser(complexDataBackupFolderPath, codePackagePath))
            {
                backupParser.StateManager.TryAddStateSerializer<User>(new UserSerializer());

                int totalUsers = 0;

                backupParser.TransactionApplied += (sender, args) =>
                {
                    foreach (var reliableStateChange in args.Changes)
                    {
                        if (reliableStateChange.Name == DictionaryName)
                        {
                            foreach (var change in reliableStateChange.Changes)
                            {
                                // we can't link against User project as that will make it easy to load the dll.
                                bool isAddChange = GenericUtils.IsSubClassOfGeneric(change.GetType(), typeof(NotifyDictionaryItemAddedEventArgs<,>));
                                if (isAddChange)
                                {
                                    // make sure that this is Add event with User type.
                                    if (change.GetType().ToString().Contains("UserType.User"))
                                    {
                                        totalUsers++;
                                    }
                                }
                            }
                        }
                    }
                };

                await backupParser.ParseAsync(CancellationToken.None);

                Assert.IsTrue(totalUsers > 0, "Could not parse any user from backup");
            }
        }

        [TestMethod]
        public async Task BackupParser_EachTransactionHasRightChanges()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
                int totalDictionaryAdds = 0;
                int totalTransactionsSeenForDictionary = 0;
                int lastKeyAdded = -1;

                backupParser.TransactionApplied += (sender, args) =>
                {
                    foreach (var reliableStateChange in args.Changes)
                    {
                        if (reliableStateChange.Name == DictionaryName)
                        {
                            foreach (var change in reliableStateChange.Changes)
                            {
                                var addChange = change as NotifyDictionaryItemAddedEventArgs<long, long>;
                                if (null != addChange)
                                {
                                    totalDictionaryAdds++;
                                    Assert.IsTrue(addChange.Key > lastKeyAdded,
                                        string.Format("Found wrong changes within a transaction : False : key added {0} > lastKeyAdded {1}",
                                            totalTransactionsSeenForDictionary, lastKeyAdded));
                                }
                            }

                            Assert.AreEqual(NumOperationsPerTransaction, reliableStateChange.Changes.Count(), "Wrong number of changes within a transaction");

                            totalTransactionsSeenForDictionary += 1;
                        }
                    }
                };

                await backupParser.ParseAsync(CancellationToken.None);

                Assert.AreEqual(TotalDictionaryInserts, totalDictionaryAdds, "Not able to collect all Dictionary change events");
                Assert.AreEqual(TotalDictionaryTransactions, totalTransactionsSeenForDictionary, "Wrong number of transactions");
            }
        }

        [TestMethod]
        public async Task BackupParser_EachTransactionHasRightChangesEvenWithBlockingTransactionAppliedEvents()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
                int totalDictionaryAdds = 0;
                int totalTransactionsSeenForDictionary = 0;
                int lastKeyAdded = -1;
                var rand = new Random();

                backupParser.TransactionApplied += (sender, args) =>
                {
                    foreach (var reliableStateChange in args.Changes)
                    {
                        if (reliableStateChange.Name == DictionaryName)
                        {
                            foreach (var change in reliableStateChange.Changes)
                            {
                                var addChange = change as NotifyDictionaryItemAddedEventArgs<long, long>;
                                if (null != addChange)
                                {
                                    totalDictionaryAdds++;
                                    Assert.IsTrue(addChange.Key > lastKeyAdded,
                                        string.Format("Found wrong changes within a transaction : False : key added {0} > lastKeyAdded {1}",
                                            totalTransactionsSeenForDictionary, lastKeyAdded));
                                }
                            }

                            Assert.AreEqual(NumOperationsPerTransaction, reliableStateChange.Changes.Count(), "Wrong number of changes within a transaction");

                            totalTransactionsSeenForDictionary += 1;
                        }
                    }

                    // sleep for 10-200 ms.
                    Thread.Sleep(rand.Next(10, 200));
                };

                await backupParser.ParseAsync(CancellationToken.None);

                Assert.AreEqual(TotalDictionaryInserts, totalDictionaryAdds, "Not able to collect all Dictionary change events");
                Assert.AreEqual(TotalDictionaryTransactions, totalTransactionsSeenForDictionary, "Wrong number of transactions");
            }
        }

        [TestMethod]
        public async Task BackupParser_VerifyQueuesTransaction()
        {
            using (var backupParser = new BackupParser(BackupFolderPath, ""))
            {
                int totalTransactionsSeenForQueue = 0, totalTransactionsSeenForConcQueue = 0, totalTransactionsSeen = 0;
                var rand = new Random();

                backupParser.TransactionApplied += (sender, args) =>
                {
                    foreach (var reliableStateChange in args.Changes)
                    {
                        if (reliableStateChange.Name == QueueName)
                        {
                            // Uncomment once we fire events for queues.
                            // Assert.AreEqual(NumOperationsPerTransaction, reliableStateChange.Changes.Count(), "Wrong number of changes within a transaction.");
                            totalTransactionsSeenForQueue += 1;
                        }
                        else if (reliableStateChange.Name == ConcurrentQueueName)
                        {
                            // Assert.AreEqual(NumOperationsPerTransaction, reliableStateChange.Changes.Count(), "Wrong number of changes within a transaction.");
                            totalTransactionsSeenForConcQueue += 1;
                        }
                    }

                    // sleep for 10 - 200 ms.
                    Thread.Sleep(rand.Next(10, 200));
                    totalTransactionsSeen += 1;
                };

                await backupParser.ParseAsync(CancellationToken.None);
                // Uncomment once we fire events for queues.
                // Assert.AreEqual(TotalQueueTransactions, totalTransactionsSeenForQueue, "Wrong number of transactions for queue.");
                // Assert.AreEqual(TotalQueueTransactions, totalTransactionsSeenForQueue, "Wrong number of transactions for concurrent queue.");
                Assert.AreEqual(TotalTransactions, totalTransactionsSeen, "Wrong number of total Transactions");
            }
        }
    }
}
