using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace WormPak;

public class PakFile : IDisposable
{
    public class Header
    {
        /// <summary>
        /// Must be PACK.
        /// </summary>
        public string Magic { get; set; }
        
        /// <summary>
        /// Offset of file table 
        /// </summary>
        public int Offset { get; set; }
        
        /// <summary>
        /// Size of file table.
        /// </summary>
        public int Size { get; set; }

        public int EntryCount => Size / 64;

        public static Header FromReader(BinaryReader reader)
        {
            var magic = new string(reader.ReadChars(4));
            if (magic != "PACK")
                throw new FileFormatException("File is not a pak file.");

            return new Header
            {
                Magic = magic,
                Offset = reader.ReadInt32(),
                Size = reader.ReadInt32()
            };
        }
    }
    
    public class Entry
    {
        /// <summary>
        /// File name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Offset of file
        /// </summary>
        public int Offset { get; set; }
        
        /// <summary>
        /// Size of file
        /// </summary>
        public int Size { get; set; }

        public static Entry FromReader(BinaryReader reader)
        {
            return new Entry
            {
                Name = new string(reader.ReadChars(56)).Trim().Replace("\0", string.Empty),
                Offset = reader.ReadInt32(),
                Size = reader.ReadInt32()
            };
        }
    }

    /// <summary>
    /// Underlying pak file stream.
    /// </summary>
    private FileStream? Stream;

    /// <summary>
    /// List of all files in pak file.
    /// </summary>
    public List<Entry> Entries { get; } = new();
    
    public static PakFile FromFile(string path)
    {
        var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
        var retval = new PakFile
        {
            Stream = stream
        };
        
        var header = Header.FromReader(reader);
        stream.Seek(header.Offset, SeekOrigin.Begin);
        
        for (var i = 0; i < header.EntryCount; i++)
            retval.Entries.Add(Entry.FromReader(reader));

        return retval;
    }

    /// <summary>
    /// Get underlying string contents for the file entry.
    /// </summary>
    /// <param name="entry">File entry to get contents from.</param>
    /// <returns>The file contents as a string.</returns>
    public string? GetStringContents(Entry entry)
    {
        if (Stream == null) return null;
        Stream.Seek(entry.Offset, SeekOrigin.Begin);
        using var reader = new BinaryReader(Stream, Encoding.ASCII, leaveOpen: true);
        return new string(reader.ReadChars(entry.Size));
    }
    public byte[]? GetBinaryContents(Entry entry)
    {
        if (Stream == null) return null;
        Stream.Seek(entry.Offset, SeekOrigin.Begin);
        using var reader = new BinaryReader(Stream, Encoding.ASCII, leaveOpen: true);
        return reader.ReadBytes(entry.Size);
    }

    internal Image? GetTgaContents(Entry entry)
    {
        if (Stream == null) throw new NullReferenceException("Stream is null, should not be.");
        Stream.Seek(entry.Offset, SeekOrigin.Begin);
        return DmitryBrant.ImageFormats.TgaReader.Load(Stream);
    }

    internal Image? GetPcxContents(Entry entry)
    {
        if (Stream == null) throw new NullReferenceException("Stream is null, should not be.");
        Stream.Seek(entry.Offset, SeekOrigin.Begin);
        return DmitryBrant.ImageFormats.PcxReader.Load(Stream);
    }

    internal Image? GetImageContents(Entry entry)
    {
        /* sourced from https://github.com/rds1983/StbSharp/blob/master/Samples/StbSharp.ImageConverter/Form1.cs */
        var _loadedImage = StbSharp.StbImage.LoadFromMemory(GetBinaryContents(entry), StbSharp.StbImage.STBI_rgb_alpha);

        // Convert to bgra
        var data = new byte[_loadedImage.Data.Length];
        Array.Copy(_loadedImage.Data, data, data.Length);

        for (var i = 0; i < _loadedImage.Width * _loadedImage.Height; ++i)
        {
            var r = data[i * 4];
            var g = data[i * 4 + 1];
            var b = data[i * 4 + 2];
            var a = data[i * 4 + 3];


            data[i * 4] = b;
            data[i * 4 + 1] = g;
            data[i * 4 + 2] = r;
            data[i * 4 + 3] = a;
        }

        // Convert to Bitmap
        var bmp = new Bitmap(_loadedImage.Width, _loadedImage.Height, PixelFormat.Format32bppArgb);
        var bmpData = bmp.LockBits(new Rectangle(0, 0, _loadedImage.Width, _loadedImage.Height), ImageLockMode.WriteOnly,
            bmp.PixelFormat);

        Marshal.Copy(data, 0, bmpData.Scan0, bmpData.Stride * bmp.Height);
        bmp.UnlockBits(bmpData);

        return bmp;
    }

    public void Dispose()
    {
        Stream?.Close();
    }

    internal Stream? GetEntryStream(Entry entry)
    {
        var memory = GetBinaryContents(entry);
        if (memory == null) return null;
        return new MemoryStream(memory);
    }
}