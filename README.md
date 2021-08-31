# DotNet.AsyncProcessing
.Net async processing primitives

# AsyncPipe

BackPressure primitive build on top of the System Channels.
Example:

```csharp
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
var pipe = AsyncPipe
    .Of<int>(5)
    .Out(2, TimeSpan.FromSeconds(2))
    .Pipe;

pipe.Consume(async (nums, ct) =>
{
    await Task.Delay(100, ct);
    Console.WriteLine("[ " + string.Join(", ", nums) + " ]");
}, skipEmptyOutput: true);

await pipe.PushAsync(1, TimeSpan.FromSeconds(1));
await pipe.PushAsync(2, TimeSpan.FromSeconds(1));
await pipe.PushAsync(3, TimeSpan.FromSeconds(1));
```
