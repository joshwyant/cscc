using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CParser.Helpers
{
    public class AsyncStreamBlock<TInput, TOutput> : IPropagatorBlock<TInput, TOutput>
    {
        // This block's input buffer
        protected BufferBlock<TInput> TargetBlock { get; }
        // This block's output buffer
        protected BufferBlock<TOutput> SourceBlock { get; }
        protected CancellationToken CancellationToken { get; }
        protected Func<IAsyncStream<TInput>, IAsyncEnumerable<TOutput>> Function { get; }
        public AsyncStreamBlock(Func<IAsyncStream<TInput>, IAsyncEnumerable<TOutput>> function, CancellationToken cancellation = default)
        {
            Function = function;
            TargetBlock = new BufferBlock<TInput>();
            SourceBlock = new BufferBlock<TOutput>();
            CancellationToken = cancellation;

            var enumerable = TargetBlock.ReceiveAllAsync(CancellationToken);
            var stream = new AsyncStreamWrapper<TInput>(enumerable);
            var task = SourceBlock
                .PostAllAsync(
                    Function(stream), CancellationToken)
                .ContinueWith(delegate {
                    SourceBlock.Complete();
                });
            Completion = SourceBlock.Completion;
        }

        public Task Completion { get; }

        public void Complete()
        {
            TargetBlock.Complete();
        }

        public TOutput ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
        {
            return (SourceBlock as ISourceBlock<TOutput>)!
                .ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public void Fault(Exception exception)
        {
            (TargetBlock as ITargetBlock<TInput>).Fault(exception);
        }

        public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            return SourceBlock.LinkTo(target, linkOptions);
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source, bool consumeToAccept)
        {
            return (TargetBlock as ITargetBlock<TInput>)!
                .OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            (SourceBlock as ISourceBlock<TOutput>)!
                .ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            return (SourceBlock as ISourceBlock<TOutput>)
                .ReserveMessage(messageHeader, target);
        }
    }
}