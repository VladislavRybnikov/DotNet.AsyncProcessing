using System;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Agents
{
    public abstract class Message<T>
    {
        public T Data { get; }
        
        public Message(T data)
        {
            Data = data;
        }
         
        public abstract ValueTask Reply<TResponse>(TResponse response);
    }
    
    internal class FireAndForgetMessage<T> : Message<T>
    {
        public FireAndForgetMessage(T data) : base(data)
        {
        }

        public override ValueTask Reply<TResponse>(TResponse response) => new();
    }

    public interface IErrorPropagation
    {
        void SetException(Exception e);
        void SetCancelled();
    }

    internal class ReplyMessage<T, TReply> : Message<T>, IErrorPropagation
    {
        private readonly TaskCompletionSource<TReply> _replyTo;

        public ReplyMessage(T data, TaskCompletionSource<TReply> replyTo) : base(data)
        {
            _replyTo = replyTo;
        }

        public override ValueTask Reply<TResponse>(TResponse response)
        {
            if (response is TReply reply)
            {
                _replyTo.SetResult(reply);
            }

            return new();
        }

        public void SetException(Exception e)
        {
            _replyTo.SetException(e);
        }

        public void SetCancelled()
        {
            _replyTo.SetCanceled();
        }
    }
    
    internal class ReplyQueueMessage<T, TReply> : Message<T>
    {
        private readonly ReplyQueue<TReply> _replyTo;

        public ReplyQueueMessage(T data, ReplyQueue<TReply> replyTo) : base(data)
        {
            _replyTo = replyTo;
        }

        public override ValueTask Reply<TResponse>(TResponse response)
        {
            if (response is TReply reply)
            {
                return _replyTo.Push(reply);
            }

            return new();
        }
    }
}