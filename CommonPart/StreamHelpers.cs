using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CommonPart
{
    public static class StreamHelpers
    {


        public static string ToHex(this ReadOnlyMemory<byte> src)
        {
            var result = new StringBuilder();

            foreach (var b in src.ToArray())
                result.Append(b.ToString("x2"));

            return result.ToString();
        }

        public static void WriteInt(this byte[] data, int value, int offset = 0)
        {
            data[offset++] = (byte)value;
            data[offset++] = (byte)(value >> 8);
            data[offset++] = (byte)(value >> 16);
            data[offset] = (byte)(value >> 24);
        }
        
        public static async ValueTask<byte[]> ReadFromSocketAsync(this Stream stream, int size)
        {
            var result = new byte[size];
            var read = 0;

            while (read < size)
            {
                var readChunkSize = await stream.ReadAsync(result, read, result.Length - read);

                if (readChunkSize == 0)
                    throw new Exception("Disconnected");

                read += readChunkSize;
            }

            return result;
        }


        public static uint ParseUint(this byte[] data)
        {
            var offset = 0;
            return (uint)(data[offset++] + data[offset++] * 256 + data[offset++] * 65536 + data[offset] * 16777216);
        }

        public static async ValueTask<int> ReadIntFromSocketAsync(this Stream stream)
        {
            var bytes = await ReadFromSocketAsync(stream, 4);
            return (int)bytes.ParseUint();
        }
        
    }
}