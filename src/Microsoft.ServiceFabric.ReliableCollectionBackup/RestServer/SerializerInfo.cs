// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    /// <summary>
    /// Information about serializers in <see cref="Configuration"/>
    /// </summary>
    internal class SerializerInfo
    {
        /// <summary>
        /// Fully qualified name of state.
        /// E.g. in Api : TryAddStateSerializer<User>(new UserSerializer());
        /// Fully qualified name of `User` class is StateFullyQualifiedTypeName.
        /// </summary>
        public string StateFullyQualifiedTypeName { get; set; }

        /// <summary>
        /// Fully qualified name of serializer to register with <see cref="StateFullyQualifiedTypeName"/>.
        /// E.g. in Api : TryAddStateSerializer<User>(new UserSerializer());
        /// Fully qualified name of `UserSerializer` class is SerializerFullyQualifiedTypeName.
        /// </summary>
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
