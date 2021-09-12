using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.AsyncProcessing.Agents;

namespace DotNet.AsyncProcessing.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // await PipeExamples.SimplePipeExample();

            //await SimpleAgentExample();

            await ParallelAgentExample();
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
    }
}