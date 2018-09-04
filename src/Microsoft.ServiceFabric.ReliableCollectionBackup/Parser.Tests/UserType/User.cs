using System.Xml;
using System.Runtime.Serialization;

namespace Microsoft.ServiceFabric.Tools.RCBackupParserTypes.UserType
{
    /// <summary>
    /// User type
    /// </summary>
    [DataContract(Name = "User", Namespace = "http://www.rcbackupparser.com")]
    public class User : IExtensibleDataObject
    {
        /// <summary>
        /// Name
        /// </summary>
        [DataMember]
        public string Name { get; private set ; }

        /// <summary>
        /// Age
        /// </summary>
        [DataMember]
        public uint Age { get; private set; }
        /// <summary>
        /// Address
        /// </summary>
        [DataMember]
        public Address Address { get; private set; }

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

        private ExtensionDataObject extensionData_Value;
    }

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
        public string Street { get; private set; }
        /// <summary>
        /// Country
        /// </summary>
        [DataMember]
        public string Country { get; private set; }
        /// <summary>
        /// PinCode
        /// </summary>
        [DataMember]
        public uint PinCode { get; private set; }

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
