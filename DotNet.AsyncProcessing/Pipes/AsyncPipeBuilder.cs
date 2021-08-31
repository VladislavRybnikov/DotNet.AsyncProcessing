using System;
using DotNet.AsyncProcessing.Pipes.Impl;

namespace DotNet.AsyncProcessing.Pipes
{
    public class AsyncPipeBuilder<T> : IAsyncPipeBuilder<T>
    {
        private int _degreeOfParallelism;
        private Func<T, int> _groupSelector;
        private int _inSize;
        private int _outSize;
        private TimeSpan? _timeout;

        private readonly Lazy<IAsyncPipe<T>> _builtPipe;

        public AsyncPipeBuilder(int inSize = 1)
        {
            _inSize = inSize;

            _builtPipe = new Lazy<IAsyncPipe<T>>(() => _degreeOfParallelism switch
            {
                > 1 => ParallelAsyncPipe(),
                _ => AsyncPipe()
            });
        }

        public IAsyncPipeBuilder<T> In(int size)
        {
            _inSize = size;
            return this;
        }

        public IAsyncPipeBuilder<T> Out(int size, TimeSpan? timeout = null)
        {
            _outSize = size;
            _timeout = timeout;
            return this;
        }

        public IAsyncPipeBuilder<T> ParallelBy(Func<T, int> groupSelector, int maxParallel)
        {
            _groupSelector = groupSelector;
            _degreeOfParallelism = maxParallel;
            return this;
        }

        public IAsyncPipe<T> Pipe => _builtPipe.Value;
        
        private IAsyncPipe<T> AsyncPipe() => new AsyncPipe<T>(_inSize, _outSize, _timeout);

        private IAsyncPipe<T> ParallelAsyncPipe() => new ParallelAsyncPipe<T>(_inSize, _groupSelector, _degreeOfParallelism);
    }
}