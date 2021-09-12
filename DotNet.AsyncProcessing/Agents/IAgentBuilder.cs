using System;
using DotNet.AsyncProcessing.Agents.Impl;

namespace DotNet.AsyncProcessing.Agents
{
    internal class StatelessAgentBuilder : IStatelessAgentBuilder
    {
        public IAgentBuilder<TMsg> Of<TMsg>() => new AgentBuilder<TMsg>(() => new StatelessAgent<TMsg>());
    }
    
    internal class AgentBuilder<TMsg> : IAgentBuilder<TMsg>, IParallelAgentBuilder<TMsg>, IBatchedAgentBuilder<TMsg>
    {
        private readonly Func<IAgent<TMsg>> _builder;

        public AgentBuilder(Func<IAgent<TMsg>> builder)
        {
            _builder = builder;
        }

        public IAgent<TMsg> Create()
        {
            return _builder();
        }

        public IParallelAgentBuilder<TMsg> ParallelBy(Func<TMsg, int> selector, int maxGroups)
        {
            return new AgentBuilder<TMsg>(() => new ParallelAgent<TMsg>(1, selector, maxGroups));
        }

        public IBatchedAgentBuilder<TMsg> Batch(int batch)
        {
            throw new NotImplementedException();
        }

        IBatchedAgent<TMsg> IBatchedAgentBuilder<TMsg>.Create()
        {
            throw new NotImplementedException();
        }
    }

    public interface IAgentBuilder<TState, TMsg> : IAgentCreation<TMsg>
    {
        IBatchedAgentBuilder<TState, TMsg> Batch(int batch);
    }
    
    public interface IAgentBuilder<TMsg> : IAgentCreation<TMsg>
    {
        IParallelAgentBuilder<TMsg> ParallelBy(
            Func<TMsg, int> selector, int maxGroups);
            
        IBatchedAgentBuilder<TMsg> Batch(int batch);

    }
    
    public interface IStatelessAgentBuilder
    {
        IAgentBuilder<TMsg> Of<TMsg>();
    }
    
    public interface IStatefulAgentBuilder<TState>
    {
        IAgentBuilder<TState,TMsg> Of<TMsg>();
    }
    
    public interface IAgentCreation<TMsg>
    {
        IAgent<TMsg> Create();
    }
    
    public interface IAgentCreation<TState, TMsg>
    {
        IAgent<TState, TMsg> Create();
    }
    
    public interface IParallelAgentBuilder<TMsg> : IAgentCreation<TMsg>
    {
    }
    
    public interface IBatchedAgentBuilder<TMsg>
    {
        IBatchedAgent<TMsg> Create();
    }
    
    public interface IParallelAgentBuilder<TState, TMsg>
        : IAgentCreation<TState, TMsg>
    {
    }
    
    public interface IBatchedAgentBuilder<TState, TMsg>
        : IAgentCreation<TState, TMsg>
    {}
}