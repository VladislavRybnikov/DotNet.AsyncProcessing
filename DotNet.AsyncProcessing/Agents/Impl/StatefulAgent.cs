using System.Threading;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Agents.Impl
{
    internal class StatefulAgent<TState, TMsg> : AgentBase<TMsg>, IAgent<TState, TMsg>
    {
        public TState State { get; private set; }

        public StatefulAgent(int mailboxCapacity = 1) : base(mailboxCapacity)
        {
        }
        
        public ValueTask Post(TMsg item) => PostBase(item);

        public ValueTask<TResponse> Ask<TResponse>(TMsg item) => AskBase<TResponse>(item);

        public void Start(
            AgentBehaviour<TState, TMsg> behaviour,
            CancellationToken ct = default)
        {
            Loop(async msg =>
            {
                State = await behaviour(State, msg, ct);
            }, ct);
        }
    }
}