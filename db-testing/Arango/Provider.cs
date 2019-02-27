using System.Net;
using System.Threading.Tasks;
using ArangoDB.Client;
using DocumentDbTest.Abstractions;
using Microsoft.Extensions.Configuration;

namespace DocumentDbTest.Arango
{
    public class Provider : IProvider
    {
        private readonly ArangoDbFactory _factory;
        private bool _ensuredCollectionExists = false;
        
        public Provider(IConfiguration configuration)
        {
            _factory = new ArangoDbFactory(new DatabaseSharedSetting
            {
                  Url = configuration["Url"],
                  Database = configuration["Database"],
                  Credential = new NetworkCredential(configuration["Username"], configuration["Password"]),
                  DisableChangeTracking = true
            });
        }
        
        public async Task<T> Store<T>(string primaryKey, T state) where T : class, new()
        {
            using (var db = _factory.Create())
            {
                var wrapped = Document.Wrap(primaryKey, state);

                await EnsureCollectionExists<T>(db);

                var (success, result) = await db.TryUpsertDocumentAsync(wrapped);
                if (!success)
                {
                    throw new ArangoServerException(result);
                }
            }

            return state;
        }

        public async Task<T> Get<T>(string primaryKey) where T : class, new()
        {
            using (var db = _factory.Create())
            {
                var (_, wrapped) = await db.TryGetDocumentAsync<Document<T>>(primaryKey);
                return wrapped.State;
            }
        }
        
        private async Task EnsureCollectionExists<T>(IArangoDatabase db)
        {
            if (!_ensuredCollectionExists)
            {
                _factory.AddCollectionNameMap<Document<T>>();
                
                var (success, result) = await db.TryCreateCollectionAsync<Document<T>>();
                if (!success)
                {
                    throw new ArangoServerException(result);
                }
            
                _ensuredCollectionExists = true;    
            }
        }
    }
}