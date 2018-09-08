// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.UserType
{
    /// <summary>
    /// Address type
    /// </summary>
    [DataContract(Name = "Address", Namespace = "http://www.rcbackupparser.com")]
    public class Address : IExtensibleDataObject
    {
        /// <summary>
        /// Street
        /// </summary>
        [DataMember]
        public string Street { get; internal set; }
        /// <summary>
        /// Country
        /// </summary>
        [DataMember]
        public string Country { get; internal set; }
        /// <summary>
        /// PinCode
        /// </summary>
        [DataMember]
        public uint PinCode { get; internal set; }

        /// <summary>
        /// ExtensionData
        /// </summary>
        public ExtensionDataObject ExtensionData { get => extensionData_Value; set => extensionData_Value = value; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Address()
        {
            this.Street = "Apollo Bandar";
            this.Country = "India";
            this.PinCode = 400011;
        }

        private ExtensionDataObject extensionData_Value;
    }
}
