using System;
using System.IO;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    internal class SerializerInfo
    {
        public SerializerInfo(string fullyQualifiedName)
        {
            this.FullyQualifiedName = fullyQualifiedName;
        }

        public string FullyQualifiedName { get; set; }

        internal void Validate()
        {
            if (String.IsNullOrWhiteSpace(this.FullyQualifiedName))
            {
                throw new InvalidDataException("FullyQualifedName can not be null or empty.");
            }
        }
    }
}
