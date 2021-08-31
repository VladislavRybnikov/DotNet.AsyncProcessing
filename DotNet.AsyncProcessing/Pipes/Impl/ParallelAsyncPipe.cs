using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Pipes.Impl
{
    internal class ParallelAsyncPipe<T> : IAsyncPipe<T>
    {
        private readonly Func<T, int> _groupSelector;
        private readonly int _maxGroups;
        private readonly IReadOnlyDictionary<int, IAsyncPipe<T>> _parallelPipes;

        public ParallelAsyncPipe(int maxBatchSize, Func<T, int> groupSelector, int maxGroups)
        {
            _groupSelector = groupSelector;
            _maxGroups = maxGroups;
            _parallelPipes = Enumerable.Range(0, maxGroups)
                .ToDictionary(g => g, _ 
                    => AsyncPipe.Of<T>(maxBatchSize).Pipe);
        }

        public void Push(T item)
        {
            var group = _groupSelector(item) % _maxGroups;
            if (_parallelPipes.TryGetValue(group, out var pipe))
            {
                pipe.Push(item);
            }
        }

        public ValueTask PushAsync(T item, TimeSpan delay)
        {
            var group = _groupSelector(item) % _maxGroups;
            if (_parallelPipes.TryGetValue(group, out var pipe))
            {
                return pipe.PushAsync(item, delay);
            }
            return new();
        }

        public ValueTask PushAsync(T item)
        {
            var group = _groupSelector(item) % _maxGroups;
            if (_parallelPipes.TryGetValue(group, out var pipe))
            {
                return pipe.PushAsync(item);
            }
            return new();
        }

        public bool Consume(PipeConsumer<T> consumer, bool suppressConsumerExceptions = false, bool skipEmptyOutput = false)
        {
            var result = true;
            foreach (var (_, pipe) in _parallelPipes)
            {
                result &= pipe.Consume(consumer, suppressConsumerExceptions, skipEmptyOutput);
            }

            return result;
        }
        
        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_parallelPipes.Values
                .Select(async p => await p.DisposeAsync()));
        }
    }
}