using Xunit;
using CParser.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace tests
{
    public class StreamTests
    {
        [Fact]
        public async Task TestAsyncStreamWrapper()
        {
            var ints = new AsyncStreamWrapper<int>(generator(), 0xFF);

            for (int i = 0; i < 5; i++)
            {
                Assert.False(await ints.Eof());
                Assert.Equal(await ints.Peek(), i);
                Assert.Equal(await ints.Read(), i);
            }
            ints.PutBack(4);
            ints.PutBack(3);
            for (int i = 3; i < 10; i++)
            {
                Assert.False(await ints.Eof());
                Assert.Equal(await ints.Peek(), i);
                Assert.Equal(await ints.Read(), i);
            }

            Assert.True(await ints.Eof());
            Assert.Equal(await ints.Peek(), ints.Sentinel);
            Assert.True(await ints.Eof());
        }

        [Fact]
        public async Task TestPostAllAsync()
        {
            var block = new BufferBlock<int>();

            var postingTask = block.PostAllAsync(generator())
                .ContinueWith(_ => block.Complete());

            Assert.Equal(
                new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}, 
                await block.ReceiveAllAsync().AsList());

            await postingTask;
        }

        [Fact]
        public async Task TestReceiveAllAsync()
        {
            var block = new BufferBlock<int>();

            var postingTask = block.PostAllAsync(generator())
                .ContinueWith(_ => block.Complete());

            Assert.Equal(
                new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}, 
                await block.ReceiveAllAsync().AsList());

            await postingTask;
        }

        [Fact]
        public async Task TestAsyncStreamBlock()
        {
            AsyncStreamFunc<int, string> func = asyncStreamFuncLessThan6;
            var block = func.Buffered(0xFF);

            var postingTask = block.PostAllAsync(generator())
                .ContinueWith(_ => block.Complete());

            Assert.Equal(
                new[] {"0", "1", "2", "3", "4", "5"}, 
                await block.ReceiveAllAsync().AsList());

            await postingTask;
        }

        [Fact]
        public async Task TestComposeAndStream()
        {
            AsyncStreamFunc<int, string> func = asyncStreamFuncLessThan6;
            var pipeline = func.Buffered(0xFF).ComposeAndChain(asyncStreamFuncBangs, asyncStreamFuncQueries);

            await pipeline.PostAllAsync(generator());
            pipeline.Complete();

            Assert.Equal(
                new[] {"0!?", "1!?", "2!?", "3!?", "4!?", "5!?"},
                await pipeline.ReceiveAllAsync().AsList());
        }

        [Fact]
        public async Task TestChainFunctions()
        {
            AsyncStreamFunc<int, string> func = asyncStreamFuncLessThan6;
            var pipeline = func.Chain(asyncStreamFuncBangs).Chain(asyncStreamFuncQueries);

            var result = pipeline(new AsyncStreamWrapper<int>(generator(), 0xFF));

            Assert.Equal(
                new[] {"0!?", "1!?", "2!?", "3!?", "4!?", "5!?"},
                await result.AsList());
        }

        protected async IAsyncEnumerable<int> generator()
        {
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                yield return i;
            }
        }

        protected async IAsyncEnumerable<string> asyncStreamFuncLessThan6(IAsyncStream<int> input)
        {
            while (!await input.Eof())
            {
                if (await input.Peek() <= 5)
                {
                    yield return (await input.Read()).ToString();
                }
                else
                {
                    yield break;
                }
            }
        }

        protected async IAsyncEnumerable<string> asyncStreamFuncBangs(IAsyncStream<string> input)
        {
            while (!await input.Eof())
            {
                yield return (await input.Read()) + "!";
            }
        }

        protected async IAsyncEnumerable<string> asyncStreamFuncQueries(IAsyncStream<string> input)
        {
            while (!await input.Eof())
            {
                yield return (await input.Read()) + "?";
            }
        }
    }
}
