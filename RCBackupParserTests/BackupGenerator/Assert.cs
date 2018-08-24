// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;

namespace Microsoft.ServiceFabric.Tools.RCBackupGenerator
{
    internal static class Assert
    {
        public static void AreEqual<T>(T a, T b, string msg = "are not equal") where T : IEquatable<T>
        {
            if (a != null)
            {
                if (!a.Equals(b))
                {
                    throw new Exception(string.Format("{0} != {1} : {2}", a, b, msg));
                }
            }
            else if (b != null)
            {
                throw new Exception(string.Format("{0} != {1} : {2}", a, b, msg));
            }
        }

        public static void IsFalse(bool x)
        {
            Assert.AreEqual(x, false, "False expected; Found True");
        }
    }
}