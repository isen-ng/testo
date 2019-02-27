using System.Runtime.Serialization;
using ArangoDB.Client.Data;

namespace DocumentDbTest.Arango.Models
{
    public class GetCollectionResult : BaseResult
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "isSystem")]
        public bool IsSystem { get; set; }

        [DataMember(Name = "status")]
        public int Status { get; set; }

        [DataMember(Name = "type")]
        public int Type { get; set; }   
    }
}