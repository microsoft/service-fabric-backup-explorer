using System;
using Microsoft.ServiceFabric.Tools.RCBackupParser;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Data.Notifications;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.ServiceFabric.Tools.RCBackupParserCmd
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Pass backup folder path");
                Environment.Exit(1);
            }

            var backupFolderPath = args[0];

            Task.Run(async () =>
            {
                using (var backupParser = new RCBackupParser.RCBackupParser(backupFolderPath, ""))
                {
                    int totalDictionaryAdds = 0;

                    backupParser.TransactionApplied += (sender, transactionArgs) =>
                    {
                        foreach (var reliableStateChange in transactionArgs.Changes)
                        {
                            if (reliableStateChange.Name == new Uri("urn:testDictionary"))
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

                    if (totalDictionaryAdds != 64)
                    {
                        throw new Exception("Not able to collect all Dictionary change events");
                    }
                    else
                    {
                        Console.WriteLine("Successfully seen 64 add events.");
                    }
                }

            }).Wait();
        }
    }
}
