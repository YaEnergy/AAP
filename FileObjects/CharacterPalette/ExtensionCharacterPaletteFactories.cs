using AAP.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Files
{
    public static class CharacterPaletteEncoderFactory
    {
        /// <summary>
        /// Creates a FileObjectEncoder for Character Palette
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static FileObjectEncoder<CharacterPalette> New(string ext, CharacterPalette palette, Stream stream)
        {
            return ext.ToLower() switch
            {
                ".txt" => new TextCharacterPaletteEncoder(palette, stream),
                ".aappal" => new AAPPALCharacterPaletteEncoder(palette, stream),
                _ => throw new Exception($"No Character Palette encoder exists for extension {ext}!"),
            };
        }
    }

    public static class CharacterPaletteDecoderFactory
    {
        /// <summary>
        /// Creates a FileObjectDecoder for Character Palette
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static FileObjectDecoder<CharacterPalette> New(string ext, Stream stream)
        {
            return ext.ToLower() switch
            {
                ".txt" => new TextCharacterPaletteDecoder(stream),
                ".aappal" => new AAPPALCharacterPaletteDecoder(stream),
                _ => throw new Exception($"No Character Palette decoder exists for extension {ext}!"),
            };
        }
    }
}
