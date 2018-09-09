using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Tests
{
    [TestClass]
    public class ConfigParserTests
    {
        [TestMethod]
        public void Config_ValidConfigParseSuccess()
        {
            string test = @"{
                'BackupChainInfos' : [
                    {
                        'AppName' : 'MyAppName',
                        'ServiceName' : 'MyServiceName',
                        'BackupChainPath' : 'c:/sad/mybackup_paths/',
                        'CodePackagePath' : 'c:/ad/df/code/',
                        'Serializers' : [ {
                                            'StateFullyQualifiedTypeName': 'StateTypeName, AssemblyName',
                                            'SerializerFullyQualifiedTypeName': 'SerializerTypeName, AssemblyName'
                                          },
                                          {
                                            'StateFullyQualifiedTypeName': 'StateTypeName2, AssemblyName2',
                                            'SerializerFullyQualifiedTypeName': 'SerializerTypeName2, AssemblyName2'
                                          } ]
                    }
                ]
            }";

            var configParser = GetConfigParser(test);
            var backupChainInfos = configParser.GetConfigration().GetBackupChainInfos();
            Assert.AreEqual(1, backupChainInfos.Count(), "One backupchaininfo expected.");

            var backupInfo = backupChainInfos.First();
            Assert.AreEqual("MyAppName", backupInfo.AppName);
            Assert.AreEqual("MyServiceName", backupInfo.ServiceName);
            Assert.AreEqual("c:/ad/df/code/", backupInfo.CodePackagePath);
            Assert.AreEqual("c:/sad/mybackup_paths/", backupInfo.BackupChainPath);
            Assert.AreEqual(2, backupInfo.Serializers.Count(), "2 serializers expected");
            var serializers = backupInfo.Serializers.ToList();
            Assert.AreEqual("StateTypeName, AssemblyName", serializers[0].StateFullyQualifiedTypeName);
            Assert.AreEqual("StateTypeName2, AssemblyName2", serializers[1].StateFullyQualifiedTypeName);
            Assert.AreEqual("SerializerTypeName, AssemblyName", serializers[0].SerializerFullyQualifiedTypeName);
            Assert.AreEqual("SerializerTypeName2, AssemblyName2", serializers[1].SerializerFullyQualifiedTypeName);
        }

        [TestMethod]
        public void Config_MissingNonRequiredValuesConfigParseSuccess()
        {
            string test = @"{
                'BackupChainInfos' : [
                    {
                        'BackupChainPath' : 'c:/sad/mybackup_paths/',
                        'CodePackagePath' : 'c:/ad/df/code/'
                    }
                ]
            }";

            var configParser = GetConfigParser(test);
            var backupChainInfos = configParser.GetConfigration().GetBackupChainInfos();
            Assert.AreEqual(1, backupChainInfos.Count(), "One backupchaininfo expected.");

            var backupInfo = backupChainInfos.First();
            Assert.IsFalse(String.IsNullOrWhiteSpace(backupInfo.AppName));
            Assert.IsFalse(String.IsNullOrWhiteSpace(backupInfo.ServiceName));
            Assert.AreEqual("c:/ad/df/code/", backupInfo.CodePackagePath);
            Assert.AreEqual("c:/sad/mybackup_paths/", backupInfo.BackupChainPath);
            Assert.AreEqual(0, backupInfo.Serializers.Count(), "No serializers expected");
        }

        [TestMethod]
        public void Config_BackupPathRequiredValuesConfigParseFailure()
        {
            string test = @"{
                'BackupChainInfos' : [
                    {
                        'CodePackagePath' : 'c:/ad/df/code/'
                    }
                ]
            }";

            Assert.ThrowsException<InvalidDataException>(() => GetConfigParser(test));
        }

        [TestMethod]
        public void Config_CodePackagePathRequiredValuesConfigParseFailure()
        {
            string test = @"{
                'BackupChainInfos' : [
                    {
                        'BackupChainPath' : 'c:/ad/df/code/'
                    }
                ]
            }";

            Assert.ThrowsException<InvalidDataException>(() => GetConfigParser(test));
        }

        [TestMethod]
        public void Config_SerializersNameNonEmptyTest()
        {
            string test = @"{
                'BackupChainInfos' : [
                    {
                        'BackupChainPath' : 'c:/sad/mybackup_paths/',
                        'CodePackagePath' : 'c:/ad/df/code/',
                        'Serializers' : [ {
                                            'StateFullyQualifiedTypeName': 'StateTypeName, AssemblyName',
                                            'SerializerFullyQualifiedTypeName': 'SerializerTypeName, AssemblyName'
                                          },
                                          {
                                            'StateFullyQualifiedTypeName': '',
                                            'SerializerFullyQualifiedTypeName': 'SerializerTypeName2, AssemblyName2'
                                          } ]
                    }
                ]
            }";

            Assert.ThrowsException<InvalidDataException>(() => GetConfigParser(test));
        }

        ConfigParser GetConfigParser(string json)
        {
            // convert string to stream
            byte[] byteArray = Encoding.ASCII.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            return new ConfigParser(stream);
        }
    }
}
