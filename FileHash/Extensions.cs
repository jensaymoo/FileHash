using AutoMapper;
using System.Linq.Expressions;

namespace FileHash
{
    internal static class Extensions
    {
        public static async Task<byte[]> ReadBatchAsync(this Stream stream, int batchSize)
        {
            byte[] buffer = new byte[batchSize];
            int numRead;

            numRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            var zippedBuffer = new byte[numRead];
            Array.Copy(buffer, zippedBuffer, numRead);

            return await Task.FromResult(zippedBuffer);
        }
    }
}
