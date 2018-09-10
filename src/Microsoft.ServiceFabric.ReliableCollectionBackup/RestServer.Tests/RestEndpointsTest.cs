using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

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
            this.VerifyResponse(resContent, 2, 0);
        }

        void VerifyResponse(string jsonContent, int numValues, int startKey)
        {
            var config = JObject.Parse(jsonContent);
            JToken values = null;

            System.Diagnostics.Debugger.Launch();

            if (config.TryGetValue(ValueName, System.StringComparison.OrdinalIgnoreCase, out values))
            {
                Assert.AreEqual(numValues, values.Count(), "Number of expected values is not same");
                var keys = new List<int>(values.Select(value => value.Value<int>(KeyName)));
                for (int i = 0; i < numValues; ++i)
                {
                    Assert.AreEqual(startKey + i, keys[i], "Keys are not in sorted order");
                }
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
