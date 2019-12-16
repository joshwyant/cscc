using System;
using Xunit;
using CParser.Lexing;
using CParser.Translation;
using System.IO;
using System.Linq;
using CParser.Helpers;
using static CParser.Lexing.Terminal;
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

            await block.PostAllAsync(generator());
            block.Complete();

            var results = new List<int>();
            while (await block.OutputAvailableAsync())
            {
                results.Add(await block.ReceiveAsync());
            }

            Assert.Equal(new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}, results);
        }

        [Fact]
        public async Task TestReceiveAllAsync()
        {
            var block = new BufferBlock<int>();

            await block.PostAllAsync(generator());
            block.Complete();

            var results = new List<int>();
            await foreach (var result in block.ReceiveAllAsync())
            {
                results.Add(result);
            }

            Assert.Equal(new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}, results);
        }

        [Fact]
        public async Task TestAsyncStreamBlock()
        {
            var block = new AsyncStreamBlock<int, string>(asyncStreamFunc, 0xFF);

            await block.PostAllAsync(generator());
            block.Complete();

            var results = new List<string>();
            await foreach (var result in block.ReceiveAllAsync())
            {
                results.Add(result);
            }

            Assert.Equal(new[] {"0", "1", "2", "3", "4", "5"}, results);
        }

        protected async IAsyncEnumerable<int> generator()
        {
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                yield return i;
            }
        }

        protected async IAsyncEnumerable<string> asyncStreamFunc(IAsyncStream<int> input)
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
    }
}
