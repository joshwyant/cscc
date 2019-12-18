using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CParser.Translation;

namespace tests
{
    class FakeFileResolver : FileResolver
    {
        protected Dictionary<string, string> map = new Dictionary<string, string>();

        public void DefineFile(string filename, string contents)
        {
            map.Add(filename, contents);
        }

        public override BinaryReader ResolveBinaryReader(string filename, FileType fileType = FileType.Unknown, Encoding encoding = null)
        {
            if (!map.ContainsKey(filename)) throw new FileNotFoundException(filename);
            return new BinaryReader(new MemoryStream((encoding ?? Encoding.ASCII).GetBytes(map[filename])), encoding ?? Encoding.ASCII);
        }

        public override BinaryWriter ResolveBinaryWriter(string filename, FileType fileType = FileType.Unknown, FileMode fileMode = FileMode.Truncate, Encoding encoding = null)
        {
            throw new NotSupportedException();
        }

        public override TextReader ResolveTextReader(string filename, FileType fileType = FileType.Unknown, Encoding encoding = null)
        {
            if (!map.ContainsKey(filename)) throw new FileNotFoundException(filename);
            return new StringReader(map[filename]);
        }

        public override TextWriter ResolveTextWriter(string filename, FileType fileType = FileType.Unknown, FileMode fileMode = FileMode.Truncate, Encoding encoding = null)
        {
            throw new NotSupportedException();
        }
    }
}