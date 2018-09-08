// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.IO;

using Newtonsoft.Json;
using Microsoft.ServiceFabric.Data;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.UserType
{
    /// <summary>
    /// Custom serializer for Address.
    /// </summary>
    public class AddressSerializer : IStateSerializer<Address>
    {
        /// <summary>
        /// Deserialize Address from binary reader.
        /// </summary>
        /// <param name="reader">Binary reader</param>
        /// <returns>Address object read from reader</returns>
        public Address Read(BinaryReader reader)
        {
            return ((IStateSerializer<Address>)this).Read(reader);
        }

        /// <summary>
        /// Serialize address to binary writer.
        /// </summary>
        /// <param name="value">Address to serialize</param>
        /// <param name="writer">Binary writer to write</param>
        public void Write(Address value, BinaryWriter writer)
        {
            ((IStateSerializer<Address>)this).Write(value, writer);
        }

        Address IStateSerializer<Address>.Read(BinaryReader reader)
        {
            return JsonConvert.DeserializeObject<Address>(reader.ReadString());
        }

        void IStateSerializer<Address>.Write(Address value, BinaryWriter writer)
        {
            var valueStr = JsonConvert.SerializeObject(value);
            writer.Write(valueStr);
        }

        // Read overload for differential de-serialization
        Address IStateSerializer<Address>.Read(Address baseValue, BinaryReader reader)
        {
            return ((IStateSerializer<Address>)this).Read(reader);
        }

        // Write overload for differential serialization
        void IStateSerializer<Address>.Write(Address baseValue, Address newValue, BinaryWriter writer)
        {
            ((IStateSerializer<Address>)this).Write(newValue, writer);
        }
    }
}
