using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.AsyncProcessing.Agents;
using DotNet.AsyncProcessing.Pipes;

namespace DotNet.AsyncProcessing.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // await PipeExamples.SimplePipeExample();

            //await SimpleAgentExample();

            // await ParallelAgentExample();

            await WindowAgentExample();
        }
        
        private static async Task ParallelAgentExample()
        {
            var agent = Agent
                .Stateless()
                .Of<int>()
                .ParallelBy(x => x, 5)
                .Create();
            
            await AgentWork(agent);
        }
        
        private static async Task SequencedAgentExample()
        {
            var agent = Agent
                .Stateless()
                .Of<int>()
                .Create();
            
            await AgentWork(agent);
        }

        private static async Task AgentWork(IAgent<int> agent)
        {
            agent.Start(async (msg, ct) =>
            {
                await Task.Delay(1000, ct);
                Console.WriteLine($"Message: {msg.Data} - reply on Thread: {Thread.CurrentThread.ManagedThreadId}");
                
                await msg.Reply(true);
            });

            var replyQueue = new ReplyQueue<bool>(5);
            for(var i = 0; i < 5; i++) await agent.Ask(i, replyQueue);
            _ = await replyQueue.WaitAll();
        }

        private static async Task WindowAgentExample()
        {
            var processingAgent = Agent.Stateless().Of<int>().ParallelBy(x => x, 5).Create();
            processingAgent.Start(async (msg, ct) =>
            {
                await Task.Delay(1000, ct);
                Console.WriteLine($"Message processed - {msg.Data}");
                await msg.Reply(msg.Data);
            });
            
            var consumeAgent = Agent.Stateful<(DateTimeOffset LastEvent, ReplyQueue<int> Processing)>()
                .Of<int>()
                .Create();
            consumeAgent.Start(async (state, msg, ct) =>
            {
                var (lastEventTime, processingQueue) = state;
                if (processingQueue.IsFull || (DateTimeOffset.Now - lastEventTime) > TimeSpan.FromSeconds(1))
                {
                    var processed = await processingQueue.WaitAll();
                    Console.WriteLine($"Wait messages processed - {string.Join(", ", processed)}");
                    
                    processingQueue = new ReplyQueue<int>(3);
                }

                await processingAgent.Ask(msg.Data, processingQueue);
                return (DateTimeOffset.Now, processingQueue);
            }, (DateTimeOffset.Now, new ReplyQueue<int>(3)));

            await consumeAgent.Post(1);
            await consumeAgent.Post(2);
            await consumeAgent.Post(3);
            await consumeAgent.Post(4);
            await Task.Delay(1020);
        }
    }
}