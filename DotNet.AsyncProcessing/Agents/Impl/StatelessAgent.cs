using System.Threading;

namespace DotNet.AsyncProcessing.Agents.Impl
{
    internal class StatelessAgent<TMsg> : AgentBase<TMsg>, IAgent<TMsg>
    {
        public StatelessAgent(int mailboxCapacity = 1) : base(mailboxCapacity)
        {
        }
        
        public void Start(AgentBehaviour<TMsg> behaviour, CancellationToken ct = default)
        {
            Loop(async msg =>
            {
                await behaviour(msg, ct);
            }, ct);
        }
    }
}