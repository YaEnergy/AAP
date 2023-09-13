using AAP.BackgroundTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Files
{
    public abstract class FileObjectEncoder<T>
    {
        public T FileObject { get; set; }
        public Stream EncodeStream { get; }

        public FileObjectEncoder(T fileObject, Stream stream)
        {
            FileObject = fileObject;
            EncodeStream = stream;
        }

        public abstract void Encode();

        public abstract Task EncodeAsync(BackgroundTaskToken? taskToken = null);

        /// <summary>
        /// Closes the FileObjectEncoder Stream!
        /// </summary>
        public void Close()
            => EncodeStream.Close();
    }

    public abstract class FileObjectDecoder<T>
    {
        public Stream DecodeStream { get; }

        public FileObjectDecoder(Stream stream)
        {
            DecodeStream = stream;
        }

        public abstract T Decode();

        public abstract Task<T> DecodeAsync(BackgroundTaskToken? taskToken = null);

        /// <summary>
        /// Closes the FileObjectDecoder Stream!
        /// </summary>
        public void Close()
            => DecodeStream.Close();
    }
}
