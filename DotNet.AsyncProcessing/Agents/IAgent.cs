using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Agents
{
    public interface IAgent<TState, TMsg>
    {
        TState State { get; }
        
        ValueTask Post(TMsg item);
        
        ValueTask<TResponse> Ask<TResponse>(TMsg item);

        void Start(AgentBehaviour<TState, TMsg> behaviour,
            TState initialState = default,
            CancellationToken cts = default);
    }
    public interface IAgent<TMsg>
    {
        ValueTask Post(TMsg item);

        ValueTask<TResponse> Ask<TResponse>(TMsg item);
        
        ValueTask Ask<TResponse>(TMsg item, ReplyQueue<TResponse> replyTo);

        void Start(AgentBehaviour<TMsg> behaviour,
            CancellationToken ct = default);
    }
    
    public interface IBatchedAgent<TMsg>
    {
        ValueTask Post(TMsg item);
        
        ValueTask<TResponse> Ask<TResponse>(TMsg item);
        
        ValueTask<IEnumerable<TResponse>> AskRange<TResponse>(params TMsg[] items);
        
        void Start(AgentBehaviour<IEnumerable<TMsg>> behaviour,
            CancellationToken cts = default);
    }
}