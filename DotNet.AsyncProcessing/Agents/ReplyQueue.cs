using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Agents
{
    public class ReplyQueue<T>
    {
        private readonly int? _maxSize;
        private int _requestedSize;
        
        private readonly Channel<T> _channel;

        public bool IsFull => _requestedSize > _maxSize - 1;
        
        public ReplyQueue(int? maxSize = null)
        {
            _maxSize = maxSize;
            _channel = maxSize is { } number 
                ? Channel.CreateBounded<T>(number) 
                : Channel.CreateUnbounded<T>();
        }

        public void Request()
        {
            if (_requestedSize > _maxSize - 1)
            {
                throw new InvalidOperationException();
            }
            
            Interlocked.Increment(ref _requestedSize);
        }

        public ValueTask Push(T reply) => _channel.Writer.WriteAsync(reply);

        public async ValueTask<IReadOnlyCollection<T>> WaitAll()
        {
            var minSize = _maxSize is {} s && _requestedSize > _maxSize 
                ? s 
                : _requestedSize;

            return await _channel.Reader.ReadManyAsync(minSize, minSize).ToListAsync();
        }
    }
}