// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.UserType
{
    /// <summary>
    /// Utility functions.
    /// </summary>
    public static class UserUtitilites
    {
        /// <summary>
        /// Returns if two users are equal or not.
        /// </summary>
        /// <param name="a">First User</param>
        /// <param name="b">Second User</param>
        /// <returns>True if both users are equal otherwise false.</returns>
        public static bool Compare(User a, User b)
        {
            return a.Name == b.Name &&
                a.Age == b.Age &&
                Compare(a.Address, b.Address);
        }

        static bool Compare(Address a, Address b)
        {
            return a.Street == b.Street &&
                a.Country == b.Country &&
                a.PinCode == b.PinCode;
        }
    }
}
