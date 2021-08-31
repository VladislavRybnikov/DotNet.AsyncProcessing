using System;

namespace DotNet.AsyncProcessing.Pipes
{
    public interface IAsyncPipeBuilder<T>
    {
        IAsyncPipeBuilder<T> In(int size);
        IAsyncPipeBuilder<T> Out(int size, TimeSpan? timeout = null);

        IAsyncPipe<T> Pipe { get; }
    }
}