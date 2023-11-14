using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Depcat.IO
{
    public sealed unsafe class DataStream : IDisposable
    {
        private readonly static Dictionary<Encoding, int> widthMap = new Dictionary<Encoding, int>()
        {
            { Encoding.UTF8, 1 },
            { Encoding.Unicode, 2 },
            { Encoding.BigEndianUnicode, 2 },
            { Encoding.UTF32, 4 },
            { Encoding.ASCII, 1 },
            { Encoding.Latin1, 1 },
            { Encoding.Default, Encoding.Default.GetByteCount("a") },
        };

        public byte[] Data;
        public int Position;
        private readonly GCHandle GCHandle;
        private byte* Pointer;

        /// <summary>
        /// Initializes a new instance of the DataStream class with the specified byte array.
        /// </summary>
        /// <param name="data">The byte array containing the data.</param>
        public DataStream(byte[] data)
        {
            Data = data;
            Position = 0;
            GCHandle = GCHandle.Alloc(Data, GCHandleType.Pinned);
            Pointer = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(Data, 0);
        }

        /// <summary>
        /// Initializes a new instance of the DataStream class with the content of the provided MemoryStream.
        /// </summary>
        /// <param name="memoryStream">The MemoryStream containing the data.</param>
        public DataStream(MemoryStream memoryStream)
        {
            if (memoryStream == null)
                throw new ArgumentNullException(nameof(memoryStream));

            Data = memoryStream.ToArray();
            Position = 0;
            GCHandle = GCHandle.Alloc(Data, GCHandleType.Pinned);
            Pointer = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(Data, 0);
        }

        /// <summary>
        /// Initializes a new instance of the DataStream class with the content of the provided FileStream.
        /// </summary>
        /// <param name="fileStream">The FileStream containing the data.</param>
        public DataStream(FileStream fileStream)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                Data = memoryStream.ToArray();
            }

            Position = 0;
            GCHandle = GCHandle.Alloc(Data, GCHandleType.Pinned);
            Pointer = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(Data, 0);
        }

        /// <summary>
        /// Reads a value of type T from the data stream.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <returns>The value read from the data stream.</returns>
        /// <remarks>
        /// Will auto-default to reading UTF8 if <typeparamref name="T"/> is <see cref="string"/>.
        /// For better string reading, use <see cref="ReadString(Encoding, bool)"/>
        /// </remarks>
        public T Read<T>()
        {
            if (typeof(T) == typeof(string))
            {
                // We assume that the string is UTF8
                string str = Marshal.PtrToStringUTF8((nint)Pointer + Position);
                Position += Encoding.UTF8.GetByteCount(str) + 1;
                return Unsafe.As<string, T>(ref str);
            }

            T ret = Unsafe.Read<T>(Pointer + Position);
            Position += Unsafe.SizeOf<T>();
            return ret;
        }

        /// <summary>
        /// Reads an array of type T from the data stream.
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="count">The number of elements to read.</param>
        /// <returns>An array of elements read from the data stream.</returns>
        public T[] Read<T>(int count)
        {
            T[] items = new T[count];
            for (int i = 0; i < count; i++)
                items[i] = Read<T>();
            return items;
        }

        /// <summary>
        /// Peeks an array of type T from the data stream.
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="count">The number of elements to read.</param>
        /// <returns>An array of elements peeked from the data stream.</returns>
        public T Peek<T>()
        {
            int pos = Position;
            T ret = Read<T>();
            Position = pos;
            return ret;
        }

        /// <summary>
        /// Peeks an array of type T from the data stream.
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="count">The number of elements to read.</param>
        /// <returns>An array of elements peeked from the data stream.</returns>
        public T[] Peek<T>(int count)
        {
            int pos = Position;
            T[] items = Read<T>(count);
            Position = pos;
            return items;
        }

        /// <summary>
        /// Reads a string from the data stream.
        /// </summary>
        /// <param name="encoding">The encoding to use for reading the string. Default is UTF-8.</param>
        /// <param name="prefixedLen">A flag indicating whether the string length is prefixed. Default is false.</param>
        /// <returns>The string read from the data stream.</returns>
        public string ReadString(Encoding encoding = null, bool prefixedLen = false)
        {
            encoding ??= Encoding.UTF8;

            int width = widthMap[encoding];
            int length = 0;
            if (prefixedLen)
            {
                length = Read7BitEncodedInt();
            }
            else
            {
                while (true)
                {
                    int currentOffset = Position + (length * width);

                    bool isEndOfString =
                        (width == 1 && *(Pointer + currentOffset) == 0) ||
                        (width == 2 && *(ushort*)(Pointer + currentOffset) == 0) ||
                        (width == 4 && *(int*)(Pointer + currentOffset) == 0);

                    if (isEndOfString)
                        break;

                    length++;
                }
                length *= width;
            }

            string str = encoding.GetString(Pointer + Position, length);

            if (prefixedLen)
            {
                Position += length;
            }
            else
            {
                Position += length + width;
            }

            return str;
        }

        /// <summary>
        /// Reads a string from the data stream.
        /// </summary>
        /// <param name="length">The length of the string to be read.</param>
        /// <param name="encoding">The encoding to use for reading the string. Default is UTF-8.</param>
        /// <returns>The string read from the data stream.</returns>
        public string ReadString(int length, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            length *= widthMap[encoding];

            string str = encoding.GetString(Pointer + Position, length);

            Position += length;
            return str;
        }

        /// <summary>
        /// Peek a string from the data stream.
        /// </summary>
        /// <param name="encoding">The encoding to use for reading the string. Default is UTF-8.</param>
        /// <param name="prefixedLen">A flag indicating whether the string length is prefixed. Default is false.</param>
        /// <returns>The string peeked from the data stream.</returns>
        public string PeekString(Encoding encoding = null, bool prefixedLen = false)
        {
            int pos = Position;
            string str = ReadString(encoding, prefixedLen);
            Position = pos;
            return str;
        }

        /// <summary>
        /// Peeks a string from the data stream.
        /// </summary>
        /// <param name="length">The length of the string to be read.</param>
        /// <param name="encoding">The encoding to use for reading the string. Default is UTF-8.</param>
        /// <returns>The string peeked from the data stream.</returns>
        public string PeekString(int length, Encoding encoding = null)
        {
            int pos = Position;
            string str = ReadString(length, encoding);
            Position = pos;
            return str;
        }

        /// <summary>
        /// Reads a 7-bit encoded integer from the current position in the data stream.
        /// </summary>
        /// <returns>The 7-bit encoded integer read from the data stream.</returns>
        public int Read7BitEncodedInt()
        {
            int result = 0;
            int shift = 0;

            byte b;
            do
            {
                b = *(Pointer + Position);
                result |= (b & 0x7F) << shift;
                shift += 7;
                Position += 1;
            } while ((b & 0x80) != 0);

            return result;
        }

        /// <summary>
        /// Writes a value of type T to the data stream.
        /// </summary>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        /// <param name="value">The value to write to the data stream.</param>
        /// <remarks>
        /// Unlike <see cref="Read{T}()"/>, this does not have default string functionality.
        /// Use <see cref="WriteString(string, Encoding, bool)"/>
        /// </remarks>
        public void Write<T>(T value)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                throw new ArgumentException("T cannot be or contain any references");

            Unsafe.Write<T>(Pointer + Position, value);
            Position += Unsafe.SizeOf<T>();
        }

        /// <summary>
        /// Writes a string to the data stream.
        /// </summary>
        /// <param name="str">The string to write to the data stream.</param>
        /// <param name="encoding">The encoding to use for writing the string. Default is UTF-8.</param>
        /// <param name="prefixedLen">A flag indicating whether the string length is prefixed. Default is false.</param>
        public void WriteString(string str, Encoding encoding = null, bool prefixedLen = false)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            int width = encoding.IsSingleByte
                ? 1
                : (encoding == Encoding.Unicode || encoding == Encoding.BigEndianUnicode)
                    ? 2
                    : 4;

            byte[] stringBytes = encoding.GetBytes(str);

            if (prefixedLen)
            {
                int len = stringBytes.Length;
                Write7BitEncodedInt(len);
            }

            // Write the string bytes to memory
            for (int i = 0; i < stringBytes.Length; i++)
            {
                *(Pointer + Position) = stringBytes[i];
                Position++;
            }

            if (!prefixedLen)
            {
                // Write null terminator
                for (int i = 0; i < width; i++)
                {
                    *(Pointer + Position) = 0;
                    Position++;
                }
            }
        }

        /// <summary>
        /// Writes a 7-bit encoded integer to the data stream.
        /// </summary>
        /// <param name="value">The 7-bit encoded integer to write.</param>
        public void Write7BitEncodedInt(int value)
        {
            do
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;

                // Set the high bit to indicate more bytes are left standing
                if (value != 0)
                    b |= 0x80; 

                *(Pointer + Position) = b;
                Position++;
            } while (value != 0);
        }

        /// <summary>
        /// Releases the resources used by the DataStream.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            GCHandle.Free();
            Pointer = null;
        }

        // If forgotten to dispose.
        ~DataStream()
        {
            Dispose();
        }
    }
}
