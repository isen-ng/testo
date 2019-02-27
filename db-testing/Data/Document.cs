using System;
using System.Runtime.Serialization;

namespace DocumentDbTest.Data
{
    [DataContract]
    public class Document
    {
        [DataMember]
        public string CollationKey1 { get; set; }
        
        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public string Description { get; set; }
        
        [DataMember]
        public DateTime Timestamp { get; set; }
    }
}