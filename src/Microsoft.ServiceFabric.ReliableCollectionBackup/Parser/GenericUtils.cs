using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    static class GenericUtils
    {
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
