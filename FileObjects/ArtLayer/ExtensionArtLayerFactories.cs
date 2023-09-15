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
    public static class ArtLayerDecoderFactory
    {
        /// <summary>
        /// Creates a FileObjectDecoder for Art Layers
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static FileObjectDecoder<ArtLayer> New(string ext, Stream stream)
        {
            return ext.ToLower() switch
            {
                ".txt" => new TextArtLayerDecoder(stream),
                ".bmp" => new BitmapArtLayerDecoder(new(), stream),
                ".png" => new PngArtLayerDecoder(new(), stream) ,
                ".jpg" => new JpegArtLayerDecoder(new(), stream),
                _ => throw new Exception($"No ArtLayer decoder exists for extension {ext}!"),
            };
        }

        /// <summary>
        /// Creates a FileObjectDecoder that uses a ImageArtLayerConverter for Art Layers 
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="stream"></param>
        /// <param name="imageLayerConverter"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static FileObjectDecoder<ArtLayer> New(string ext, Stream stream, ImageArtLayerConverter imageLayerConverter)
        {
            return ext.ToLower() switch
            {
                ".txt" => new TextArtLayerDecoder(stream),
                ".bmp" => new BitmapArtLayerDecoder(new(), stream),
                ".png" => new PngArtLayerDecoder(new(), stream),
                ".jpg" => new JpegArtLayerDecoder(new(), stream),
                _ => throw new Exception($"No ArtLayer decoder exists for extension {ext}!"),
            };
        }
    }
}
