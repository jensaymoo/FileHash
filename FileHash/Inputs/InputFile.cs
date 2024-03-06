using Autofac.Core.Resolving.Middleware;

namespace FileHash.Inputs
{
    internal class InputFile : IInput, IDisposable
    {
        IConfigProvider configProvider;
        Configuration configuration;
        public InputFile(IConfigProvider provider)
        {
            configProvider = provider;
            configuration = configProvider.GetConfiguration(new BaseCommandLineValidator());

            sourceStream = new FileStream(configuration.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: configuration.BatchSize, useAsync: true);

        }
        FileStream sourceStream;

        public bool CanRead => sourceStream?.CanRead ?? false;

        public async Task<byte[]> GetNextBatchBytesAsync()
        {
            if (CanRead)
            {
                byte[] buffer = new byte[configuration.BatchSize];
                int numRead;

                numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length);

                var zippedBuffer = new byte[numRead];
                Array.Copy(buffer, zippedBuffer, numRead);

                return await Task.FromResult(zippedBuffer);
            }
            else
            {
                throw new InvalidOperationException("stream cant read");
            }
        }

        public long GetBytesCount() 
        {
            if (CanRead)
            {
                return sourceStream.Length;
            }
            else
            {
                throw new InvalidOperationException("stream cant read");
            }
        }

        public void Dispose()
        {
            sourceStream?.Close();
            sourceStream?.Dispose();
        }

        public Stream OpenStream()
        {
            return (sourceStream as Stream);
        }
    }
}
