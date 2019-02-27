using System;
using System.Text;
using ArangoDB.Client;

namespace DocumentDbTest.Arango
{
    public class ArangoDbFactory
    {
        private readonly DatabaseSharedSetting _settings;

        public ArangoDbFactory(DatabaseSharedSetting settings)
        {
            _settings = settings;

            // https://github.com/ra0o0f/arangoclient.net/issues/125
            // we need to handle errors and ArangoServerException does not allow us to do that
            // so we opt not to throw exceptions and handle the results by ourselves
            _settings.ThrowForServerErrors = false;
        }

        public ArangoDatabase Create()
        {
            return new ArangoDatabase(_settings);
        }

        public ArangoDbFactory AddCollectionNameMap<TType>()
        {
            var collectionName = ResolveTypeName(typeof(TType));
            _settings.Collection.ChangeCollectionPropertyForType<TType>(s => { s.CollectionName = collectionName; });

            return this;
        }
        
        private static string ResolveTypeName(Type type)
        {
            if (type.GenericTypeArguments.Length == 0)
            {
                return type.Name;
            }

            var backTickIndex = type.Name.IndexOf('`');
            var nameBuilder = new StringBuilder(type.Name.Substring(0, backTickIndex));

            foreach (var genericType in type.GenericTypeArguments)
            {
                nameBuilder.Append(ResolveTypeName(genericType));
            }

            return nameBuilder.ToString();
        }
    }
}