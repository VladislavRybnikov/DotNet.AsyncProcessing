using System;

namespace DotNet.AsyncProcessing.Pipes
{
    public class AsyncPipeBuilder<T> : IAsyncPipeBuilder<T>
    {
        private int _inSize;
        private int _outSize;
        private TimeSpan? _timeout;

        public AsyncPipeBuilder(int inSize = 1)
        {
            _inSize = inSize;
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

        public IAsyncPipe<T> Pipe => new AsyncPipeImpl<T>(_inSize, _outSize, _timeout);
    }
}