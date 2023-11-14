using System.Linq;

namespace Depcat.IO.TMod
{
    public readonly struct Signature
    {
        public readonly byte[] Bytes;

        public Signature(byte[] bytes)
            => Bytes = bytes;

        public override string ToString()
            => string.Join(string.Empty, Bytes.Select(x => x.ToString("X")));
    }
}
