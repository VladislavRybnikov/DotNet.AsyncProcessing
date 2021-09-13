using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Agents.Impl
{
    internal class ParallelAgent<TMsg> : IAgent<TMsg>
    {
        private readonly IReadOnlyDictionary<int, IAgent<TMsg>> _agents;
        private readonly Func<TMsg, int> _groupSelector;
        private readonly int _maxGroups;

        public ParallelAgent(int mailboxSize, Func<TMsg, int> groupSelector, int maxGroups)
        {
            _groupSelector = groupSelector;
            _maxGroups = maxGroups;
            _agents = Enumerable.Range(0, maxGroups)
                .ToDictionary(g => g, _ 
                    => (IAgent<TMsg>)new StatelessAgent<TMsg>(mailboxSize));
        }

        public async ValueTask Post(TMsg item)
        {
            var group = _groupSelector(item) % _maxGroups;
            if (_agents.TryGetValue(group, out var agent))
            {
                await agent.Post(item);
            }
        }

        public async ValueTask<TResponse> Ask<TResponse>(TMsg item)
        {
            var group = _groupSelector(item) % _maxGroups;
            if (_agents.TryGetValue(group, out var agent))
            {
                return await agent.Ask<TResponse>(item);
            }

            return default;
        }

        public ValueTask Ask<TResponse>(TMsg item, ReplyQueue<TResponse> replyTo)
        {
            var group = _groupSelector(item) % _maxGroups;
            if (_agents.TryGetValue(group, out var agent))
            {
                return agent.Ask(item, replyTo);
            }

            return new();
        }

        public void Start(AgentBehaviour<TMsg> behaviour, CancellationToken ct = default)
        {
            foreach (var (_, agent) in _agents)
            {
                agent.Start(behaviour, ct);
            }
        }
    }
}