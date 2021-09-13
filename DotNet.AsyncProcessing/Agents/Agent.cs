using System.Threading;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Agents
{
    public delegate ValueTask<TState> AgentBehaviour<TState, TMsg>(
        TState state, Message<TMsg> message, CancellationToken ct = default);


    public delegate ValueTask AgentBehaviour<TMsg>(
        Message<TMsg> message, CancellationToken ct = default);

    public static class Agent
    {
        public static IStatelessAgentBuilder Stateless() => new StatelessAgentBuilder();

        public static IStatefulAgentBuilder<TState> Stateful<TState>()
        {
            return new StatefulAgentBuilder<TState>();
        }
    }
}