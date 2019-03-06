// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Controllers
{
    internal class Utilities
    {
        /// <summary>
        /// Copies source directory recursively to destination.
        /// This method overwrites existing files in destination.
        /// </summary>
        /// <param name="source">Source directory</param>
        /// <param name="destination">Destination directory</param>
        /// <returns>True if operation succeeded</returns>
        internal static async Task CopyDirectory(string source, string destination)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(source);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + source);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string destFilePath = Path.Combine(destination, file.Name);
                file.CopyTo(destFilePath, false);
            }

            // Copy subdirectories.
            foreach (DirectoryInfo subdir in dirs)
            {
                string destFolder = Path.Combine(destination, subdir.Name);
                await CopyDirectory(subdir.FullName, destFolder);
            }
        }
    }
}