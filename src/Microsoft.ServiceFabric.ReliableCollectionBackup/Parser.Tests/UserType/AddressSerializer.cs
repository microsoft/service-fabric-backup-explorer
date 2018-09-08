// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.IO;

using Microsoft.ServiceFabric.Data;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.UserType
{
    /// <summary>
    /// Custom serializer for Address.
    /// </summary>
    public class AddressSerializer : IStateSerializer<Address>
    {
        Address IStateSerializer<Address>.Read(BinaryReader reader)
        {
            var value = new Address();
            value.Street = reader.ReadString();
            value.Country = reader.ReadString();
            value.PinCode = reader.ReadUInt32();
            return value;
        }

        void IStateSerializer<Address>.Write(Address value, BinaryWriter writer)
        {
            writer.Write(value.Street);
            writer.Write(value.Country);
            writer.Write(value.PinCode);
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
