// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.UserType
{
    /// <summary>
    /// User type used in creating and validating backups in tests and rest server.
    /// </summary>
    [DataContract(Name = "User", Namespace = "http://www.rcbackupparser.com")]
    public class User : IExtensibleDataObject
    {
        /// <summary>
        /// Name
        /// </summary>
        [DataMember]
        public string Name { get; internal set ; }

        /// <summary>
        /// Age
        /// </summary>
        [DataMember]
        public uint Age { get; internal set; }
        /// <summary>
        /// Address
        /// </summary>
        [DataMember]
        public Address Address { get; internal set; }

        /// <summary>
        /// ExtensionData
        /// </summary>
        public ExtensionDataObject ExtensionData { get => extensionData_Value; set => extensionData_Value = value; }

        /// <summary>
        /// Constructor
        /// </summary>
        public User()
        {
            this.Name = "Pandit Gangadhar Vidyadhar Mayadhar Omkarnath Shastri";
            this.Age = 31;
            this.Address = new Address();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="age">Age</param>
        /// <param name="address">Address</param>
        public User(string name, uint age, Address address)
        {
            this.Name = name;
            this.Age = age;
            this.Address = address;
        }

        /// <summary>
        /// ToString override for User
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Name: {this.Name}, Age: {this.Age}, Address: {this.Address}";
        }

        private ExtensionDataObject extensionData_Value;
    }
}
