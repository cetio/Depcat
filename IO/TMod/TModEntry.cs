using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Depcat.IO.TMod;

[StructLayout(LayoutKind.Sequential)]
public struct TModEntry
{
    private byte[] _data;
    private bool _cached;

    public string FullName { get; }
    public int Length { get; }
    public int CompressedLength { get; }
    public int Offset { get; internal set; }
    public byte[] Data
    {
        get
        {
            if (Length != CompressedLength && !_cached)
            {
                using (MemoryStream compressedStream = new MemoryStream(_data))
                using (MemoryStream decompressedStream = new MemoryStream())
                using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(decompressedStream);
                    _data = decompressedStream.ToArray();
                    _cached = true;
                }
            }

            return _data;
        }
        internal set
        {
            _data = value;
        }
    }

    public TModEntry(string fullName, int offset, int length, int compressedLength)
    {
        _data = new byte[0];
        _cached = false;
        FullName = fullName;
        Offset = offset;
        Length = length;
        CompressedLength = compressedLength;
    }
}