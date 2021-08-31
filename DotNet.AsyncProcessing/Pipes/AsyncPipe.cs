using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            => new AsyncPipe<T>(maxIn, minOut, timeout);

        /// <summary>
        /// Creates <see cref="IAsyncPipeBuilder{T}"/>.
        /// </summary>
        /// <param name="size"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAsyncPipeBuilder<T> Of<T>(int size = 1) => new AsyncPipeBuilder<T>(size);

        /// <summary>
        /// Connect one pipe to another.
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="other"></param>
        /// <param name="transform"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static IAsyncPipe<TResult> Connect<T, TResult>(
            this IAsyncPipe<T> pipe, 
            IAsyncPipe<TResult> other,
            Func<T, TResult> transform)
        {
            pipe.Consume(async (items, ct) =>
            {
                foreach (var transformed in items.Select(transform))
                {
                    await other.PushAsync(transformed);
                }
            });

            return other;
        }
    }
}