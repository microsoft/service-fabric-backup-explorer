// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;
using Microsoft.ServiceFabric.Replicator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ServiceFabric.Data.Collections.ReliableConcurrentQueue;
using Microsoft.ServiceFabric.Data.Notifications;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser.Tests
{
    [TestClass]
    public class BackupParserTests : RCBackupParserTestBase
    {
        [ClassInitialize]
        public static async Task RCBackupParserTestsClassInitialize(TestContext testContext)
        {
            await RCBackupParserTestBase.ClassInitialize(testContext);
        }

        [ClassCleanup]
        public static void RCBackupParserTestsClassCleanup()
        {
            RCBackupParserTestBase.ClassCleanup();
        }

        [TestMethod]
        public async Task RCBackupParser_FireTransactionAppliedEvents()
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
        public async Task RCBackupParser_CollectChangesInTransactionAppliedEvents()
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
                                var addChange = change as NotifyDictionaryChangedEventArgs<long, long>;
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

        [TestMethod]
        public void RCBackupParser_NoUserClassInDefaultAppDomain()
        {
            var appDomain = AppDomain.CurrentDomain;
            var userAssembly = appDomain.GetAssemblies().Where(assembly => assembly.FullName.Contains("User"));
            Assert.IsTrue(userAssembly.Count() == 0, string.Format("User assembly {0} is loaded in default app domain. " +
                "Test like RCBackupParser_AbleToFindAssemblyInCodePackage will pass even if functionality is broken.", userAssembly.FirstOrDefault()));
        }

        [TestMethod]
        public async Task RCBackupParser_AbleToFindAssemblyInCodePackage()
        {
            var complexDataBackupFolderPath = Path.Combine(ClassTestPath, @"..\UserFullBackup");
            // from this test run's bin\Debug\netstandard2.0\<testname> dir to parent of bin
            var codePackagePath = Path.Combine(ClassTestPath, @"..\..\..\..\UserType\bin\");

            using (var backupParser = new BackupParser(complexDataBackupFolderPath, codePackagePath))
            {
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
                                bool isAddChange = GenericUtils.IsSubClassOfGeneric(change.GetType(), typeof(NotifyDictionaryChangedEventArgs<,>));
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
    }
}
