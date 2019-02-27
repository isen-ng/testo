using System.Runtime.Serialization;
using ArangoDB.Client;
using Newtonsoft.Json;

namespace DocumentDbTest.Arango
{
    [DataContract]
    internal class Document<TState>
    {
        [DataMember]
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string Key { get; private set; }
        
        [DataMember]
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto, ItemTypeNameHandling = TypeNameHandling.Auto)]
        public TState State { get; private set; }

        public Document(string key, TState state)
        {
            Key = key;
            State = state;
        }
    }

    internal static class Document
    {
        public static Document<T> Wrap<T>(string key, T state)
        {
            return new Document<T>(key, state);
        }
    }
}