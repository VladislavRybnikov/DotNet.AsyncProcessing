using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DotNet.AsyncProcessing.Exceptions;

namespace DotNet.AsyncProcessing.Pipes
{
    internal class AsyncPipeImpl<T> : IAsyncPipe<T>
    {
        private readonly int _minBatchSize;
        private readonly TimeSpan? _timeout;
        private readonly Channel<T> _channel;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _ct;
        private readonly int _maxBatchSize;
        private ConsumerException _consumerException;
        private uint _consumed;
        private Task _consumeTask;

        public AsyncPipeImpl(int maxBatchSize, int minBatchSize, TimeSpan? timeout = null)
        {
            if (maxBatchSize < 1)
                throw new ArgumentOutOfRangeException(nameof(maxBatchSize), "Batch size is lower than 1");
            _maxBatchSize = maxBatchSize;
            _minBatchSize = minBatchSize;
            _timeout = timeout;
            _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(maxBatchSize)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });
            _cts = new CancellationTokenSource();
            _ct = _cts.Token;
        }
        
        public void Push(T item)
        {
            if (_consumerException is { } ex) throw ex;
            SpinWait.SpinUntil(() => _channel.Writer.TryWrite(item));
        }
        
        public async ValueTask PushAsync(T item, TimeSpan delay)
        {
            await Task.Delay(delay, _ct);
            if (_consumerException is { } ex) throw ex;
            await _channel.Writer.WriteAsync(item, _ct);
        }

        public ValueTask PushAsync(T item)
        {
            if (_consumerException is { } ex) return ValueTask.FromException(ex);
            return _channel.Writer.WriteAsync(item, _ct);
        }

        public bool Consume(PipeConsumer<T> consumer, 
            bool suppressConsumerExceptions = false,
            bool skipEmptyOutput = false)
        {
            if (Interlocked.CompareExchange(ref _consumed, 1, 0) != 0) return false;
            _consumeTask = Task.Run(async () =>
            {
                while (!_ct.IsCancellationRequested)
                {
                    try
                    {
                        var output = await _channel.Reader
                            .ReadManyAsync(_minBatchSize, _maxBatchSize, _ct, _timeout)
                            .ToArrayAsync(_ct);
                            
                        if(skipEmptyOutput && output.Length == 0) continue;
                        await consumer(output, _ct);
                    }
                    catch (ObjectDisposedException) when (_ct.IsCancellationRequested)
                    {
                        // do nothing
                    }
                    catch (Exception e)
                    {
                        if (suppressConsumerExceptions) continue;
                        _consumerException = new ConsumerException(e);
                        await DisposeAsync();
                        break;
                    }
                }
            }, _ct);

            return true;
        }
        
        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _channel.Writer.Complete();
            await _channel.Reader.Completion;
            await _consumeTask.ContinueWith(_ =>
            {
                // ignore exceptions
            });
        }
    }
}