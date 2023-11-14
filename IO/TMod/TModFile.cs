using System;
using System.IO;

namespace Depcat.IO.TMod;

public sealed class TModFile
{
    public Version ModLoaderVersion { get; internal set; }
    public Hash Hash { get; internal set; }
    public Signature Signature { get; internal set; }
    public string Name { get; internal set; }
    public string Version { get; internal set; }
    public TModEntry[] Entries { get; internal set; }

    public static bool TryReadFromPath(string path, out TModFile tmodFile)
    {
        System.Windows.Forms.MessageBox.Show("PATH", "");
        return TryReadFromArray(File.ReadAllBytes(path), out tmodFile);
    }

    public static unsafe bool TryReadFromArray(byte[] bytes, out TModFile tmodFile)
    {
        using DataStream stream = new DataStream(bytes);
        tmodFile = null;

        if (stream.ReadString(4) != "TMOD")
            return false;

        tmodFile = new TModFile();
        tmodFile.ModLoaderVersion = new Version(stream.ReadString(prefixedLen: true));
        tmodFile.Hash = new Hash(stream.Read<byte>(20));
        tmodFile.Signature = new Signature(stream.Read<byte>(256));

        // unused
        stream.Read<uint>();

        tmodFile.Name = stream.ReadString(prefixedLen: true);
        tmodFile.Version = stream.ReadString(prefixedLen: true);
        tmodFile.Entries = new TModEntry[stream.Read<int>()];

        int offset = 0;
        for (int i = 0; i < tmodFile.Entries.Length; i++)
        {
            tmodFile.Entries[i] = new TModEntry(stream.ReadString(prefixedLen: true), offset, stream.Read<int>(), stream.Read<int>());
            offset += tmodFile.Entries[i].CompressedLength;
        }

        int dsgOffset = offset;
        for (int i = 0; i < tmodFile.Entries.Length; i++)
        {
            tmodFile.Entries[i].Offset += dsgOffset;
            tmodFile.Entries[i].Data = stream.Read<byte>(tmodFile.Entries[i].CompressedLength);
        }

        return true;
    }
}
