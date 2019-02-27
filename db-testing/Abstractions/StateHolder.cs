using System;

namespace DocumentDbTest.Abstractions
{
    public class StateHolder<TState> where TState : class, new()
    {
        public TState Prepared { get; private set; }
        
        public TState Committed { get; private set; } 

        public StateHolder(TState state)
        {
            Prepared = state;
            Committed = state;
        }

        public void Prepare(TState state)
        {
            Prepared = state;
        }

        public void Prepare(Action<TState> state)
        {
            state.Invoke(Prepared ?? new TState());
        }

        public void Commit()
        {
            Committed = Prepared;
        }

        public void Rollback()
        {
            Prepared = Committed;
        }
    }
}