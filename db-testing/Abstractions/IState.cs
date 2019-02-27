using System.Threading.Tasks;

namespace DocumentDbTest.Abstractions
{
    public interface IState<TState>
    {
        Task<TState> Store(TState state);

        TState Get();
    }

    public static class GrainStateExtensions
    {
        public static bool Exists<T>(this IState<T> state)
        {
            return state.Get() != null;
        }
    }
}