using System.Threading.Tasks;
using DocumentDbTest.Abstractions;
using Marten;
using Microsoft.Extensions.Configuration;

namespace DocumentDbTest.Marten
{    
    public class Provider : IProvider
    {
        private readonly IDocumentStore _store;
        
        public Provider(IConfiguration configuration)
        {
            var options = new StoreOptions();
            options.Connection(configuration["ConnectionString"]);
            
            _store = new DocumentStore(options);
        }
        
        public async Task<T> Store<T>(string primaryKey, T state) where T : class, new()
        {
            using (var session = _store.LightweightSession())
            {
                session.Store(Document.Wrap(primaryKey, state));
                await session.SaveChangesAsync();
            }

            return state;
        }

        public async Task<T> Get<T>(string primaryKey) where T : class, new()
        {
            using (var session = _store.LightweightSession())
            {
                var document = await session.LoadAsync<Document<T>>(primaryKey);
                return document.State;
            }
        }
    }
}