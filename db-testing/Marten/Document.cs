using System.Runtime.Serialization;
using Marten.Schema;
using Newtonsoft.Json;

namespace DocumentDbTest.Marten
{
    [DataContract]
    internal class Document<TState>
    {
        [Identity]
        [DataMember]
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