using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ServiceFabric.ReliableCollectionBackup.UserType;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Tests
{
    [TestClass]
    public class RestEndpointsTest
    {
        [ClassInitialize]
        public static async Task ClassInitialize(TestContext testContext)
        {
            process = new Process();
            process.StartInfo.FileName = "Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.exe";
            process.StartInfo.Arguments = "--configfile ../../../configs/sampleconfig.json";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            await Task.Delay(10000); // wait for 10 seconds to start taking requests.
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            try
            {
                await client.GetAsync(Url + "/api/exit");
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("/api/exit exception : {0}", ex));
            }

            Console.WriteLine("StdErr: {0}", process.StandardError.ReadToEnd());
            Console.WriteLine("StdOut: {0}", process.StandardOutput.ReadToEnd());
        }

        [TestMethod]
        public async Task RestEndpoint_Top2Test()
        {
            var response = await client.GetAsync(Url + "/$query/testDictionary?$top=2");
            var resContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response : {0}", resContent);
            this.VerifySortedResponse(resContent, 2, 0);
        }

        [TestMethod]
        public async Task RestEndpoint_AddDataTest()
        {
            var postUrl = Url + "/$query";
            var jsonContent = @"[{
                'Operation': 'Add',
                'Collection': 'testDictionary',
                'Key' : -1,
                'Value' : {
                    'Name' : 'RestEndpoint_AddDataTest',
                    'Age' : 10,
                    'Address' : {
                        'Street' : 'ABC',
                        'Country' : 'India',
                        'PinCode' : 201020
                    }
                }
            }]";

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(postUrl, content);
            Assert.IsTrue(response.IsSuccessStatusCode, $"Failed to add data to dictionary : response : {response} : content {await response.Content.ReadAsStringAsync()}");
        }

        [TestMethod]
        public async Task RestEndpoint_UpdateDataTest()
        {
            var userKey = 0;
            var userAndEtag = await GetUserAndEtag(userKey);
            var etag = userAndEtag.Item2;

            // Update user
            var postUrl = Url + "/$query";
            string name = "RestEndpoint_UpdateDataTest", street = "ABC", country = "India";
            uint pinCode = 201020, age = 10;
            // {{ in below json string is for escaping { in $@ string interpolation in c#
            var jsonContent = $@"[{{
                'Operation': 'Update',
                'Collection': 'testDictionary',
                'Key' : {userKey},
                'Value' : {{
                    'Name' : '{name}',
                    'Age' : {age},
                    'Address' : {{
                        'Street' : '{street}',
                        'Country' : '{country}',
                        'PinCode' : {pinCode}
                    }}
                }},
                'Etag' : '{etag}'
            }}]";

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(postUrl, content);
            var resContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Post Response: {response} : Content {resContent}");
            Assert.IsTrue(response.IsSuccessStatusCode, $"Failed to update data to dictionary : response : {response} : content {resContent}");

            // Validate the update.
            var expectedUser = new User(name, age, new Address(street, country, pinCode));
            await this.VerifyUser(userKey, expectedUser);
        }

        [TestMethod]
        public async Task RestEndpoint_BackupFull()
        {
            var backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString("N"));
            await VerifyBackup(backupDirectory, Data.BackupOption.Full);
            Directory.Delete(backupDirectory, true);
        }

        [TestMethod]
        public async Task RestEndpoint_BackupIncremental()
        {
            var backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString("N"));
            await VerifyBackup(backupDirectory, Data.BackupOption.Full);
            var incrementalDirectory = Path.Combine(backupDirectory, "incremental");
            await VerifyBackup(incrementalDirectory, Data.BackupOption.Incremental);
            Directory.Delete(backupDirectory, true);
        }

        [TestMethod]
        public async Task RestEndpoint_BackupRequiredFields()
        {
            var twoMinuteInSecs = TimeSpan.FromMinutes(2).TotalSeconds;
            var backupDirectory = @"c:\abc";
            var bodyParams = new Dictionary<string, string> {
                { "CancellationTokenInSecs", $"{twoMinuteInSecs}" },
                { "TimeoutInSecs", $"{twoMinuteInSecs}" },
                { "BackupLocation", $"'{JsonConvert.SerializeObject(backupDirectory)}'" }
            };

            foreach (var bodyParam in bodyParams)
            {
                // send all bodyParams except bodyParam
                var sendBodyParams = bodyParams.Where(param => param.Key != bodyParam.Key);
                var sendBodyParamsJson = sendBodyParams.Select(param => $"'{param.Key}' : {param.Value}");
                var jsonContent = $"{{ {String.Join(",", sendBodyParamsJson)} }}";

                var postUrl = Url + "/api/backup/full";
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(postUrl, content);
                var resContent = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(response.IsSuccessStatusCode,
                    $"{postUrl} post request {jsonContent} success " +
                    $"even without required param : {bodyParam} : response {response} content {resContent}");
            }
        }

        [TestMethod]
        public async Task RestEndpoint_TransactionSimple()
        {
            var transactionUrl = Url + "/api/transactions/next";
            var response = await client.GetAsync(transactionUrl);
            var resContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, "Getting next transtion failed");
            Console.WriteLine($"response content : {resContent}");
            this.VerifyTransactions(resContent, 1);
        }

        [TestMethod]
        public async Task RestEndpoint_TransactionsCount()
        {
            var transactionUrl = Url + "/api/transactions/next?count=3";
            var response = await client.GetAsync(transactionUrl);
            var resContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, "Getting next transtion failed");
            this.VerifyTransactions(resContent, 3);
            Console.WriteLine($"response content : {resContent}");
        }

        [TestMethod]
        public async Task RestEndpoint_TransactionsInLoop()
        {
            for (int i = 0; i < 4; ++i)
            {
                var transactionUrl = Url + "/api/transactions/next?count=2";
                var response = await client.GetAsync(transactionUrl);
                var resContent = await response.Content.ReadAsStringAsync();
                Assert.IsTrue(response.IsSuccessStatusCode, "Getting next transtion failed");
                this.VerifyTransactions(resContent, 2);
                Console.WriteLine($"response content : {resContent}");
            }
        }

        void VerifyTransactions(string jsonContent, int expectedChanges)
        {
            var config = JObject.Parse(jsonContent);
            JToken changes = null;

            Assert.IsTrue(config.TryGetValue("changes", System.StringComparison.OrdinalIgnoreCase, out changes), "Expected 'changes' in reponse");
            Assert.AreEqual(expectedChanges, changes.Count(), "Number of expected changes is not same");

            var lastSeenKey = -1;
            foreach (var change in changes)
            {
                lastSeenKey = this.VerifyTransaction(change, lastSeenKey);
            }
        }

        int VerifyTransaction(JToken change, int lastSeenKey)
        {
            var rcName = change.Value<string>("name");
            if ("urn:testDictionary" == rcName)
            {
                var dictChanges = change.Value<JArray>("changes");
                Assert.AreEqual(8, dictChanges.Count(), "Not all dictionary changes are send");
                foreach (var dictChange in dictChanges)
                {
                    var keyChanged = dictChange.Value<int>("key");
                    // since all transactions added keys in increasing order.
                    Assert.IsTrue(lastSeenKey < keyChanged, "Transactions does not have changes in expected order.");
                    lastSeenKey = keyChanged;
                }
            }
            else
            {
                Assert.Fail($"{rcName} case is not handled.");
            }

            return lastSeenKey;
        }

        async Task VerifyBackup(string backupDirectory, Data.BackupOption backupOption)
        {
            var backupName = backupOption == Data.BackupOption.Full ? "full" : "incremental";
            var postUrl = Url + "/api/backup/" + backupName;
            var twoMinuteInSecs = TimeSpan.FromMinutes(2).TotalSeconds;
            var jsonContent = $@"{{
                'CancellationTokenInSecs' : {twoMinuteInSecs},
                'TimeoutInSecs' : {twoMinuteInSecs},
                'BackupLocation' : {JsonConvert.SerializeObject(backupDirectory, Formatting.None)}
            }}";

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(postUrl, content);
            var resContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode,
                $"{postUrl} backup post with {jsonContent} : call failed : {response} content: {resContent}");
            Assert.IsTrue(Directory.Exists(backupDirectory), "Backup folder not created");

            var verifyFiles = new List<string>();
            if (backupOption == Data.BackupOption.Full)
            {
                verifyFiles.AddRange(new string[] { "backup.metadata", "backup.log", "backup.chkpt" });
            }
            else
            {
                verifyFiles.AddRange(new string[] { "incremental.metadata", "backup.log" });
            }

            foreach (var file in verifyFiles)
            {
                Assert.AreEqual(1, Directory.GetFiles(backupDirectory, file, SearchOption.AllDirectories).Count(),
                    $"{backupName} : {file} is not present in backup folder {backupDirectory}");
            }
        }

        async Task VerifyUser(int userKey, User expectedUser)
        {
            var updatedUserAndEtag = await GetUserAndEtag(userKey);
            Assert.IsTrue(UserUtitilites.Compare(expectedUser, updatedUserAndEtag.Item1),
                       $"Both users are not same : \r\nExpected : {expectedUser}\r\n Actual : {updatedUserAndEtag.Item1}");
        }

        async Task<(User, string)> GetUserAndEtag(int userKey)
        {
            var response = await client.GetAsync(Url + "/$query/testDictionary?$top=100");
            var jsonContent = await response.Content.ReadAsStringAsync();
            return ParseUserAndEtag(jsonContent, userKey);
        }

        (User, string) ParseUserAndEtag(string jsonContent, int userKey)
        {
            var config = JObject.Parse(jsonContent);
            JToken values = null;

            Assert.IsTrue(config.TryGetValue(ValueName, System.StringComparison.OrdinalIgnoreCase, out values), "Expected 'values' in reponse");
            Assert.IsTrue(values.Count() > 0, "Number of expected values is not same");

            foreach (var value in values)
            {
                if (userKey == value.Value<int>(KeyName))
                {
                    var resUser = value.Value<JObject>("Value").ToObject<User>();
                    var etag = value.Value<string>("Etag");
                    return (resUser, etag);
                }
            }

            Assert.Fail($"Did not find any user with key : {userKey}");
            return (null, null);
        }

        void VerifySortedResponse(string jsonContent, int numValues, int startKey)
        {
            var config = JObject.Parse(jsonContent);
            JToken values = null;

            Assert.IsTrue(config.TryGetValue(ValueName, System.StringComparison.OrdinalIgnoreCase, out values), "Expected 'values' in reponse");
            Assert.AreEqual(numValues, values.Count(), "Number of expected values is not same");

            var keys = new List<int>(values.Select(value => value.Value<int>(KeyName)));
            for (int i = 0; i < numValues; ++i)
            {
                Assert.AreEqual(startKey + i, keys[i], "Keys are not in sorted order");
            }
        }

        static string Url = "http://localhost:5000";
        static string ValueName = "value";
        static string KeyName = "Key";
        static HttpClient client = new HttpClient();
        static Process process;
        static Stream outStream;
    }
}
