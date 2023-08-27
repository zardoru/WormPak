using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WormPak.Formats
{
    public class WalFile
    {
        public class WalHeader
        {
            public string Name { get; set; } = "";
            public uint Width { get; set; }
            public uint Height { get; set; }
            public uint[] Offsets { get; set; } = new uint[4];
            public string AnimName { get; set; } = "";
            public int Flags { get; set; }
            public int Contents { get; set; }
            public int Value { get; set; }

            public static WalHeader FromStream(Stream s)
            {
                using var bin = new BinaryReader(s, Encoding.ASCII, true);
                return new WalHeader
                {
                    Name = new string(bin.ReadChars(32).TakeWhile(c => c != '\0').ToArray()),
                    Width = bin.ReadUInt32(),
                    Height = bin.ReadUInt32(),
                    Offsets = new[] { bin.ReadUInt32(), bin.ReadUInt32(), bin.ReadUInt32(), bin.ReadUInt32() },
                    AnimName = new string(bin.ReadChars(32).TakeWhile(c => c != '\0').ToArray()),
                    Flags = bin.ReadInt32(),
                    Contents = bin.ReadInt32(),
                    Value= bin.ReadInt32(),
                };
            }
        }

        public WalHeader Header = new();
        public Bitmap? Bitmap;

        /// <summary>
        /// Load a WAL file from stream. Unlike PCX files, we do not need the stream to
        /// be constrained.
        /// </summary>
        /// <param name="s">Stream to load from.</param>
        /// <param name="palette">Array of 256 elements that maps a 8-bit value to a color.</param>
        /// <returns>The WAL file.</returns>
        public static WalFile FromStream(Stream s, Color[] palette)
        {
            var walStart = s.Position;
            var header = WalHeader.FromStream(s);
            var bitmap = new Bitmap((int)header.Width, (int)header.Height);

            s.Seek(walStart + header.Offsets[0], SeekOrigin.Begin);
            var bin = new BinaryReader(s, Encoding.ASCII, true);
            for (var y = 0; y < header.Height; y++)
            {
                for (var x = 0; x < header.Width; x++)
                {
                    var b = bin.ReadByte();
                    bitmap.SetPixel(x, y, palette[b]);
                }
            }

            return new WalFile
            {
                Header = header,
                Bitmap = bitmap
            };
        }
    }
}
