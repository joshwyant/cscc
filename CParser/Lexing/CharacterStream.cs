using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CParser.Helpers;

namespace CParser.Lexing
{
    public class CharacterStream : IStream<char>, IDisposable
    {
        const int BUFFER_SIZE = 4096;
        char[] buffer = new char[BUFFER_SIZE];
        public char Sentinel => '\uFFFF';
        protected TextReader Reader { get; }

        private StreamWrapper<char> Stream { get; }

        public CharacterStream(TextReader reader)
        {
            Reader = reader;
            Stream = new StreamWrapper<char>(Enumerate(), Sentinel);
        }

        private Task<int> read() => Reader.ReadAsync(buffer, 0, BUFFER_SIZE);



        protected async IAsyncEnumerable<char> Enumerate()
        {
            int count;
            var task = read();
            while ((count = await task) > 0)
            {
                task = read();
                for (var i = 0; i < count; i++)
                {
                    yield return buffer[i];
                }
            }
            yield return Sentinel;
        }

        public async Task<bool> Eof()
        {
            return await Stream.Eof();
        }

        public IAsyncEnumerator<char> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return Stream.GetAsyncEnumerator();
        }

        public async Task<char> Peek()
        {
            return await Stream.Peek();
        }

        public async Task<char> Read()
        {
            return await Stream.Read();
        }

        public void PutBack(char val)
        {
            Stream.PutBack(val);
        }

        bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                Reader.Close();
                disposed = true;
            }
        }
    }
}