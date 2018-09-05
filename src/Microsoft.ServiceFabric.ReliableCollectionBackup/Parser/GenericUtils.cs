// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    /// <summary>
    /// Utility functions for Generic types
    /// </summary>
    internal static class GenericUtils
    {
        /// <summary>
        /// Finds if <paramref name="child"/> Type is subclass of <paramref name="parent"/> Type.
        /// </summary>
        /// <param name="child">child type</param>
        /// <param name="parent">parent type</param>
        /// <returns>True if child type is subclass for parent type.</returns>
        public static bool IsSubClassOfGeneric(this Type child, Type parent)
        {
            while (child != null && child != typeof(object))
            {
                var cur = GetFullTypeDefinition(child);

                if (parent == cur || IsInInterfaces(cur, parent))
                {
                    return true;
                }

                child = child.BaseType;
            }

            return false;
        }

        private static Type GetFullTypeDefinition(Type type)
        {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }

        private static bool IsInInterfaces(Type child, Type parent)
        {
            return child.GetInterfaces().Select(i => GetFullTypeDefinition(i)).Contains(parent);
        }
    }
}
