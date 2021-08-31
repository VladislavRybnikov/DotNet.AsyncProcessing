using System;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Pipes
{
    /// <summary>
    /// Represents System <see cref="Channel{T}"/> wrapper with async processing and In/Out backpressure. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncPipe<T> : IAsyncDisposable
    {
        /// <summary>
        /// Push item to pipe.
        /// </summary>
        /// <param name="item"></param>
        void Push(T item);

        /// <summary>
        /// Push item to pipe asynchronously with delay.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        ValueTask PushAsync(T item, TimeSpan delay);
        
        /// <summary>
        /// Push item to pipe asynchronously.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ValueTask PushAsync(T item);

        /// <summary>
        /// Consume pipe items.
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="suppressConsumerExceptions"></param>
        /// <param name="skipEmptyOutput"></param>
        /// <returns></returns>
        bool Consume(PipeConsumer<T> consumer, 
            bool suppressConsumerExceptions = false,
            bool skipEmptyOutput = false);
    }
}