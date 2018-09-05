// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// Helper COM methods
    /// </summary>
    internal class ComMtaHelper
    {
        public static TResult WrapNativeSyncInvokeInMTA<TResult>(Func<TResult> func)
        {
            return RunInMTA(() => { return WrapNativeSyncInvoke<TResult>(func); });
        }

        static TResult WrapNativeSyncInvoke<TResult>(Func<TResult> func)
        {
            TResult result = func();
            return result;
        }

        static TResult RunInMTA<TResult>(Func<TResult> func)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
            {
                return func();
            }
            else
            {
                try
                {
                    Task<TResult> task = Task.Factory.StartNew<TResult>(func);
                    task.Wait();
                    return task.Result;
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            }
        }
    }
}
