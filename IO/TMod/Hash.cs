using System.Linq;

namespace Depcat.IO.TMod
{
    public readonly struct Hash
    {
        public readonly byte[] Bytes;

        public Hash(byte[] bytes)
            => Bytes = bytes;

        public override string ToString()
            => string.Join(string.Empty, Bytes.Select(x => x.ToString("X")));
    }
}
