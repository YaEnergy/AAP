using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Files
{
    public static class ASCIIArtEncoderFactory
    {
        /// <summary>
        /// Creates a FileObjectEncoder for ASCII Art
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static FileObjectEncoder<ASCIIArt> New(string ext, ASCIIArt art, Stream stream)
        {
            return ext.ToLower() switch
            {
                ".txt" => new TextASCIIArtEncoder(art, stream),
                ".aaf" => new AAFASCIIArtEncoder(art, stream),
                ".bmp" => new BitmapASCIIArtEncoder(art, stream) { EncodeOptions = new() },
                ".png" => new PngASCIIArtEncoder(art, stream) { EncodeOptions = new() },
                ".jpg" => new JpegASCIIArtEncoder(art, stream) { EncodeOptions = new() },
                ".jpeg" => new JpegASCIIArtEncoder(art, stream) { EncodeOptions = new() },
                ".jfif" => new JpegASCIIArtEncoder(art, stream) { EncodeOptions = new() },
                ".gif" => new GifASCIIArtEncoder(art, stream) { EncodeOptions = new() },
                _ => throw new Exception($"No ASCII Art encoder exists for extension {ext}!"),
            };
        }

        /// <summary>
        /// Creates a FileObjectEncoder that uses a ImageASCIIArtEncodeOptions for ASCII Art 
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="stream"></param>
        /// <param name="encodeOptions"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static FileObjectEncoder<ASCIIArt> New(string ext, ASCIIArt art, Stream stream, ImageASCIIArtEncodeOptions encodeOptions)
        {
            return ext.ToLower() switch
            {
                ".txt" => new TextASCIIArtEncoder(art, stream),
                ".aaf" => new AAFASCIIArtEncoder(art, stream),
                ".bmp" => new BitmapASCIIArtEncoder(art, stream) { EncodeOptions = encodeOptions },
                ".png" => new PngASCIIArtEncoder(art, stream) { EncodeOptions = encodeOptions },
                ".jpg" => new JpegASCIIArtEncoder(art, stream) { EncodeOptions = encodeOptions },
                ".jpeg" => new JpegASCIIArtEncoder(art, stream) { EncodeOptions = encodeOptions },
                ".jfif" => new JpegASCIIArtEncoder(art, stream) { EncodeOptions = encodeOptions },
                ".gif" => new GifASCIIArtEncoder(art, stream) { EncodeOptions = encodeOptions },
                _ => throw new Exception($"No ASCII Art encoder exists for extension {ext}!"),
            };
        }
    }

    public static class ASCIIArtDecoderFactory
    {
        /// <summary>
        /// Creates a FileObjectDecoder for ASCII Art
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static FileObjectDecoder<ASCIIArt> New(string ext, Stream stream)
        {
            return ext.ToLower() switch
            {
                ".txt" => new TextASCIIArtDecoder(stream),
                ".aaf" => new AAFASCIIArtDecoder(stream),
                ".bmp" => new BitmapASCIIArtDecoder(new(), stream),
                ".png" => new PngASCIIArtDecoder(new(), stream) ,
                ".jpg" => new JpegASCIIArtDecoder(new(), stream),
                ".jpeg" => new JpegASCIIArtDecoder(new(), stream),
                ".jfif" => new JpegASCIIArtDecoder(new(), stream),
                ".gif" => new GifASCIIArtDecoder(new(), stream),
                _ => throw new Exception($"No ASCII Art decoder exists for extension {ext}!"),
            };
        }

        /// <summary>
        /// Creates a FileObjectDecoder that uses a ImageArtLayerConverter for ASCII Art 
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static FileObjectDecoder<ASCIIArt> New(string ext, Stream stream, ImageASCIIArtDecodeOptions decodeOptions)
        {
            return ext.ToLower() switch
            {
                ".txt" => new TextASCIIArtDecoder(stream),
                ".aaf" => new AAFASCIIArtDecoder(stream),
                ".bmp" => new BitmapASCIIArtDecoder(decodeOptions, stream),
                ".png" => new PngASCIIArtDecoder(decodeOptions, stream),
                ".jpg" => new JpegASCIIArtDecoder(decodeOptions, stream),
                ".jpeg" => new JpegASCIIArtDecoder(decodeOptions, stream),
                ".jfif" => new JpegASCIIArtDecoder(decodeOptions, stream),
                ".gif" => new GifASCIIArtDecoder(decodeOptions, stream),
                _ => throw new Exception($"No ASCII Art decoder exists for extension {ext}!"),
            };
        }
    }
}
