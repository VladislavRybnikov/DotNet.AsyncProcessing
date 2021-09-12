using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotNet.AsyncProcessing.Agents.Impl
{
    internal abstract class AgentBase<TMsg> 
    {
        protected readonly Channel<Message<TMsg>> Mailbox;

        protected AgentBase(int mailboxCapacity)
        {
            Mailbox = Channel.CreateBounded<Message<TMsg>>(new BoundedChannelOptions(mailboxCapacity)
            {
                SingleReader = true
            });
        }

        protected ValueTask PostBase(TMsg item)
        {
            return Mailbox.Writer.WriteAsync(new FireAndForgetMessage<TMsg>(item));
        }
        
        protected async ValueTask<TResponse> AskBase<TResponse>(TMsg item)
        {
            var tcs = new TaskCompletionSource<TResponse>();
            await Mailbox.Writer.WriteAsync(new ReplyMessage<TMsg, TResponse>(item, tcs));
            return await tcs.Task;
        }
        
        protected ValueTask AskBase<TResponse>(TMsg item, ReplyQueue<TResponse> replyTo)
        {
            replyTo.Request();
            return Mailbox.Writer.WriteAsync(new ReplyQueueMessage<TMsg, TResponse>(item, replyTo));
        }

        protected void Loop(Func<Message<TMsg>, ValueTask> action, CancellationToken ct)
        {
            Task.Run(async () =>    
            {
                while(!ct.IsCancellationRequested)
                {
                    var msg = await Mailbox.Reader.ReadAsync(ct);

                    if (msg is not IErrorPropagation errorPropagation)
                    {
                        await action(msg);
                    }
                    else
                    {
                        try
                        {
                            await action(msg);
                        }
                        catch (TaskCanceledException)
                        {
                            errorPropagation.SetCancelled();
                        }
                        catch (Exception ex)
                        {
                            errorPropagation.SetException(ex);
                        }
                    }
                }
            }, ct);
        }
    }
}