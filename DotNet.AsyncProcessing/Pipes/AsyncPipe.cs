using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Pipes
{
    public delegate ValueTask PipeConsumer<in T>(IReadOnlyCollection<T> output, CancellationToken ct);

    /// <summary>
    /// Creates <see cref="IAsyncPipe{T}"/>.
    /// </summary>
    public static class AsyncPipe
    {
        /// <summary>
        /// Create <see cref="IAsyncPipe{T}"/> with predefined parameters.
        /// </summary>
        /// <param name="maxIn"></param>
        /// <param name="minOut"></param>
        /// <param name="timeout"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAsyncPipe<T> Create<T>(int maxIn = 1, int minOut = 1, TimeSpan? timeout = null)
            => new AsyncPipeImpl<T>(maxIn, minOut, timeout);

        /// <summary>
        /// Creates <see cref="IAsyncPipeBuilder{T}"/>.
        /// </summary>
        /// <param name="size"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAsyncPipeBuilder<T> Of<T>(int size = 1) => new AsyncPipeBuilder<T>(size);
    }
}