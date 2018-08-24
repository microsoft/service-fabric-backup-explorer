using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Tools.RCBackupParser
{
    class ComMtaHelper
    {
        public static TResult WrapNativeSyncInvokeInMTA<TResult>(Func<TResult> func, string functionTag)
        {
            return RunInMTA(() => { return WrapNativeSyncInvoke<TResult>(func, functionTag); });
        }

        static TResult WrapNativeSyncInvoke<TResult>(Func<TResult> func, string functionTag, string functionArgs = "")
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
