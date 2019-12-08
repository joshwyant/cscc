using System;
using System.IO;
using System.Text;

namespace cscc.Translation
{
    class FileResolver
    {
        public virtual TextReader ResolveTextReader(string filename, FileType fileType = default, Encoding? encoding = null)
        {
            // https://stackoverflow.com/questions/1065168/does-disposing-streamreader-close-the-stream
            return new StreamReader(File.Open(filename, FileMode.Open), encoding ?? Encoding.ASCII);
        }

        public virtual TextWriter ResolveTextWriter(string filename, FileType fileType = default, FileMode fileMode = FileMode.Truncate, Encoding? encoding = null)
        {
            return new StreamWriter(File.Open(filename, fileMode), encoding ?? Encoding.ASCII);
        }

        public virtual BinaryReader ResolveBinaryReader(string filename, FileType fileType = default, Encoding? encoding = null)
        {
            return new BinaryReader(File.Open(filename, FileMode.Open), encoding ?? Encoding.ASCII);
        }

        public virtual BinaryWriter ResolveBinaryWriter(string filename, FileType fileType = default, FileMode fileMode = FileMode.Truncate, Encoding? encoding = null)
        {
            return new BinaryWriter(File.Open(filename, fileMode), encoding ?? Encoding.ASCII);
        }
    }
}