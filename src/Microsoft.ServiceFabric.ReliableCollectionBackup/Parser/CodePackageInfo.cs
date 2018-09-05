// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    internal class CodePackageInfo : IDisposable
    {
        readonly string packagePath;

        public CodePackageInfo(string packagePath)
        {
            this.packagePath = packagePath;
            if (!String.IsNullOrWhiteSpace(this.packagePath))
            {
                AppDomain.CurrentDomain.AssemblyResolve += CodePackageAssemblyResolveHandler;
            }
        }

        private Assembly CodePackageAssemblyResolveHandler(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var assemblyToLoad = assemblyName.Name + ".dll";

            try
            {
                var assemblyPaths = Directory.EnumerateFiles(this.packagePath, assemblyToLoad, SearchOption.AllDirectories);

                foreach (var assemblyPath in assemblyPaths)
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }
            catch (Exception)
            { } // eat all the exceptions.

            return null;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CodePackageAssemblyResolveHandler;
        }
    }
}
