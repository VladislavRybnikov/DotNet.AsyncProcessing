using System.Threading;

namespace DotNet.AsyncProcessing.Agents.Impl
{
    internal class StatefulAgent<TState, TMsg> : AgentBase<TMsg>, IAgent<TState, TMsg>
    {
        public TState State { get; private set; }

        public StatefulAgent(int mailboxCapacity = 1) : base(mailboxCapacity) { }
        
        public void Start(
            AgentBehaviour<TState, TMsg> behaviour,
            TState initialState = default,
            CancellationToken ct = default)
        {
            State = initialState;
            Loop(async msg =>
            {
                State = await behaviour(State, msg, ct);
            }, ct);
        }
    }
}