using System.Threading.Tasks;

namespace DocumentDbTest.Abstractions
{
    public interface IProvider
    {
        Task<T> Store<T>(string primaryKey, T state) where T : class, new();
        
        Task<T> Get<T>(string primaryKey) where T : class, new();
    }
}