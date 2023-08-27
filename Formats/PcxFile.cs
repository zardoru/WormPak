namespace WormPak.Formats;

// getting pallete according to:
// https://pastebin.com/YuQ30Hqu
// pcx.h's dpcx_t and images.c _IMG_LoadPCX in q2pro sources
public class PcxFile
{
    public class PcxHeader
    {
        public byte Manufacturer { get; set; }
        public byte Version { get; set; }
        public byte Encoding { get; set; }
        public byte Bpp { get; set; }
        public short Xmin { get; set; }
        public short Ymin { get; set; }
        public short Xmax { get; set; }
        public short Ymax { get; set; }
        public short HRes { get; set; }
        public short VRes { get; set; }
        public byte[] Palette { get; set; } = new byte[48];
        public byte Reserved { get; set; }
        public byte ColorPlanes { get; set; }
        public short BytesPerLine { get; set; }
        public short PaletteType { get; set; }
        public byte[] Filler { get; set; } = new byte[58];

        public bool HeaderValid => !(Manufacturer != 0x0a ||
                             Version != 5 ||
                             Encoding != 1 ||
                             Bpp != 8 ||
                             Xmax >= 1023 ||
                             Ymax >= 1023 ||
                             ColorPlanes != 1);

        public bool DimensionsValid => !(Width < 1 || Height < 1 || Width > 640 || Height > 480) && 
                                       BytesPerLine >= Width;

        public bool Valid => HeaderValid && DimensionsValid;
        
        public int Width => Xmax - Xmin;
        public int Height => Ymax - Ymin;
        
        public static PcxHeader FromStream(Stream stream)
        {
            var bin = new BinaryReader(stream, System.Text.Encoding.ASCII, true);
            return new PcxHeader
            {
                Manufacturer = bin.ReadByte(),
                Version = bin.ReadByte(),
                Encoding = bin.ReadByte(),
                Bpp = bin.ReadByte(),
                Xmin = bin.ReadInt16(),
                Ymin = bin.ReadInt16(),
                Xmax = bin.ReadInt16(),
                Ymax = bin.ReadInt16(),
                HRes = bin.ReadInt16(),
                VRes = bin.ReadInt16(),
                Palette = bin.ReadBytes(48),
                Reserved = bin.ReadByte(),
                ColorPlanes = bin.ReadByte(),
                BytesPerLine = bin.ReadInt16(),
                PaletteType = bin.ReadInt16(),
                Filler = bin.ReadBytes(58)
            };
        }
    }

    public PcxHeader Header { get; private set; }  = new();
    public Color[] Color8To24Table { get; } = new Color[256];
    public Bitmap? Bitmap { get; private set; }
    
    /// <summary>
    /// Load a Quake 2 PCX texture. 
    /// </summary>
    /// <param name="stream">Stream from where to get PCX file. Constraints: Stream must end at the end of the pcx file.</param>
    /// <param name="paletteOnly">Do not read RLE encoded PCX data, just the palette.</param>
    /// <returns></returns>
    public static PcxFile? FromStream(Stream stream, bool paletteOnly = false)
    {
        var retval = new PcxFile();
        var header = PcxHeader.FromStream(stream);
        
        // bad header
        if (!header.Valid)
            return null;

        retval.Header = header;
        stream.Seek(-768, SeekOrigin.End);
        using var bin = new BinaryReader(stream, System.Text.Encoding.ASCII, true);
        for (var i = 0; i < 255; i++)
        {
            var r = bin.ReadByte();
            var g = bin.ReadByte();
            var b = bin.ReadByte();

            retval.Color8To24Table[i] = Color.FromArgb(255, r, g, b);
        }
        
        retval.Color8To24Table[255] = Color.FromArgb(0, bin.ReadByte(), bin.ReadByte(), bin.ReadByte());
        if (paletteOnly)
            return retval;

        stream.Seek(128, SeekOrigin.Begin);
        var bmp = new Bitmap(header.Width, header.Height);
        for (var y = 0; y < header.Height; y++)
        {
            for (var x = 0; x < header.BytesPerLine; )
            {
                var b = bin.ReadByte();
                var runLength = 1;
                if ((b & 0xC0) == 0xC0)
                {
                    runLength = b & 0x3F;
                    if (x + runLength > header.BytesPerLine)
                        throw new IndexOutOfRangeException("RLE packet goes past scanline length");
                    // the raw >= end is handled at the ReadByte level with an EndOfStreamException
                    b = bin.ReadByte();
                }

                while (runLength > 0)
                {
                    if (x < header.Width)
                        bmp.SetPixel(x, y, retval.Color8To24Table[b]);
                    x++;
                    runLength -= 1;
                }
            }
        }

        retval.Bitmap = bmp;
        return retval;
    }
}