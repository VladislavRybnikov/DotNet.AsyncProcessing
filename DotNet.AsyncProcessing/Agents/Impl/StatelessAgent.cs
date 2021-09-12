using System.Threading;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Agents.Impl
{
    internal class StatelessAgent<TMsg> : AgentBase<TMsg>, IAgent<TMsg>
    {
        public StatelessAgent(int mailboxCapacity = 1) : base(mailboxCapacity)
        {
        }

        public ValueTask Post(TMsg item) => PostBase(item);

        public ValueTask<TResponse> Ask<TResponse>(TMsg item) => AskBase<TResponse>(item);

        public ValueTask Ask<TResponse>(TMsg item, ReplyQueue<TResponse> replyTo) => AskBase(item, replyTo);

        public void Start(AgentBehaviour<TMsg> behaviour, CancellationToken ct = default)
        {
            Loop(async msg =>
            {
                await behaviour(msg, ct);
            }, ct);
        }
    }
}