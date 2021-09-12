using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.AsyncProcessing.Pipes;

namespace DotNet.AsyncProcessing.Examples
{
    public static class PipeExamples
    {
        public static async Task ParallelPipeExample()
        {
            // Parallel Pipe concept:
            //
            // IN:         5
            //             |
            //             V
            //           - - -
            // GROUP:   |  |  |
            //          V  V  V
            //      [___][___][___]
            //      |   ||   ||   |
            // MAX: [___][___][___]
            //       ||   ||   ||
            //       V    V    V
            //
            // OUT: [1, 2], [3, 5] [5, 6]
            
            var parallelPipe = AsyncPipe
                .Of<int>(10)
                .ParallelBy(i => i, 5)
                .Pipe;

            parallelPipe.Consume(async (nums, ct) =>
            {
                await Task.Delay(1000, ct);
                Console.WriteLine($"[ {string.Join(", ", nums)} ] Thread: {Thread.CurrentThread.ManagedThreadId}");
            }, skipEmptyOutput: true);

            await Task.WhenAll(Enumerable.Range(1, 20)
                .Select(async i => await parallelPipe.PushAsync(i)));

            await Task.Delay(5000);
        }
        
        public static async Task SimplePipeExample()
        {
            TimeSpan Sec(int s) => TimeSpan.FromSeconds(s);

            // Pipe concept:
            //
            // IN:      5
            //          |
            //          V
            // START: [___]
            //        | 4 |
            //        | 3 |
            // MAX:   [___]  Handles backpressure
            //         ||
            //         V
            // OUT: [1, 2] OR [...] on timeout
            //
            // Or: 
            // [1, 2, ..., 5] >=> [1, 2] or timeout

            //var pipe = AsyncPipe.Create<int>(5, 2, Sec(2));
            await using var pipe = AsyncPipe
                .Of<int>(5)
                .Out(2, Sec(2))
                .Pipe;

            pipe.Consume(async (nums, ct) =>
            {
                await Task.Delay(100, ct);
                Console.WriteLine("[ " + string.Join(", ", nums) + " ]");
            }, skipEmptyOutput: true);

            await pipe.PushAsync(1, Sec(1));
            await pipe.PushAsync(2, Sec(1));
            await pipe.PushAsync(3, Sec(1));

            await pipe.PushAsync(0);
            await pipe.PushAsync(0);
            await pipe.PushAsync(0);
            await pipe.PushAsync(0);
            // -- backpressure
            await pipe.PushAsync(0);
            await pipe.PushAsync(0);

            await pipe.PushAsync(4, Sec(1));
            await pipe.PushAsync(5, Sec(1));
            await pipe.PushAsync(6, Sec(1));
        }
    }
}