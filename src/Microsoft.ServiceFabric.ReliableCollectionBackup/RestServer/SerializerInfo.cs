using System;
using System.IO;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    internal class SerializerInfo
    {
        public SerializerInfo(string stateFullyQualifiedName)
        {
            this.StateFullyQualifiedTypeName = stateFullyQualifiedName;
        }

        public string StateFullyQualifiedTypeName { get; set; }
        public string SerializerFullyQualifiedTypeName { get; set; }

        internal void Validate()
        {
            if (String.IsNullOrWhiteSpace(this.StateFullyQualifiedTypeName))
            {
                throw new InvalidDataException("StateFullyQualifiedTypeName can not be null or empty.");
            }

            if (String.IsNullOrWhiteSpace(this.SerializerFullyQualifiedTypeName))
            {
                throw new InvalidDataException("SerializerFullyQualifiedTypeName can not be null or empty.");
            }
        }
    }
}
