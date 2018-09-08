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
    /// User custom serializer type.
    /// </summary>
    public class UserSerializer : IStateSerializer<User>
    {
        User IStateSerializer<User>.Read(BinaryReader reader)
        {
            var value = new User();
            value.Name = reader.ReadString();
            value.Age = reader.ReadUInt32();

            var addressSerializer = new AddressSerializer();
            value.Address = addressSerializer.Read(reader);
            return value;
        }

        void IStateSerializer<User>.Write(User value, BinaryWriter writer)
        {
            writer.Write(value.Name);
            writer.Write(value.Age);

            var addressSerializer = new AddressSerializer();
            addressSerializer.Write(value.Address, writer);
        }

        // Read overload for differential de-serialization
        User IStateSerializer<User>.Read(User baseValue, BinaryReader reader)
        {
            return ((IStateSerializer<User>)this).Read(reader);
        }

        // Write overload for differential serialization
        void IStateSerializer<User>.Write(User baseValue, User newValue, BinaryWriter writer)
        {
            ((IStateSerializer<User>)this).Write(newValue, writer);
        }
    }
}
