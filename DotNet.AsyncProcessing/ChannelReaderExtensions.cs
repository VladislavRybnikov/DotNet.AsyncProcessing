using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing
{
    internal static class ChannelReaderExtensions
    {
        public static IAsyncEnumerable<T> ReadAtLeastOneAsync<T>(this ChannelReader<T> reader, CancellationToken ct = default)
        {
            return ReadManyAsync(reader, 1, 1, ct);
        }
        
        public static async IAsyncEnumerable<T> ReadManyAsync<T>(
            this ChannelReader<T> reader, 
            int minCount, 
            int maxCount,
            [EnumeratorCancellation] CancellationToken ct = default,
            TimeSpan? timeout = null)
        {
            var i = 0;
            for (;i < minCount; i++)
            {
                if (reader.TryRead(out var fastItem)) yield return fastItem;
                else if (timeout is { } timeSpan)
                {
                    var waitToRead = reader.WaitToReadAsync(ct).AsTask();
                    var waitTimeout = Task.Delay(timeSpan, ct);

                    if (await Task.WhenAny(waitToRead, waitTimeout) == waitTimeout) break;
                    else if (reader.TryRead(out var item)) yield return item;
                }
                else yield return await reader.ReadAsync(ct);
            }

            for (; i < maxCount && reader.TryRead(out var fastItem); i++) yield return fastItem;
        }
    }
}