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
