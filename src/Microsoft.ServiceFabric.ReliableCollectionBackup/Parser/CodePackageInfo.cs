// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// Stores code package information required for parsing backup.
    /// </summary>
    internal class CodePackageInfo : IDisposable
    {
        public CodePackageInfo(string packagePath)
        {
            this.packagePath = packagePath;
            // empty packagePath are allowed for backups that have only primitive types.
            if (!String.IsNullOrWhiteSpace(this.packagePath))
            {
                AppDomain.CurrentDomain.AssemblyResolve += CodePackageAssemblyResolveHandler;
            }
        }

        private Assembly CodePackageAssemblyResolveHandler(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var assemblyToLoad = assemblyName.Name + ".exe";

            try
            {
                // Looks through all fils in code package path for required assembly.
                var assemblyPaths = Directory.EnumerateFiles(this.packagePath, assemblyToLoad, SearchOption.AllDirectories);
                
                // assemblyPaths is a lazy list, so instead of looking at Count which forces the completition of EnumerateFiles
                // just use enumeration to get first file.
                foreach (var assemblyPath in assemblyPaths)
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }
            catch (Exception)
            {
                // log here.
            }

            return null;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CodePackageAssemblyResolveHandler;
        }

        private readonly string packagePath;
    }
}
