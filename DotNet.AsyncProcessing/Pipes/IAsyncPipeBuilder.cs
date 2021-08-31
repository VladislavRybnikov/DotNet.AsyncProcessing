using System;

namespace DotNet.AsyncProcessing.Pipes
{
    public interface IAsyncPipeBuilder<T>
    {
        IAsyncPipeBuilder<T> In(int size);
        IAsyncPipeBuilder<T> Out(int size, TimeSpan? timeout = null);

        IAsyncPipeBuilder<T> ParallelBy(Func<T, int> groupSelector, int maxParallel);

        IAsyncPipe<T> Pipe { get; }
    }
}