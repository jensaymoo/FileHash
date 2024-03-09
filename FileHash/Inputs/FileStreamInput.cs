namespace FileHash.Inputs
{
    internal class FileStreamInput(IConfigProvider provider) : IInputProvider
    {
        ConfigurationFileStream? configuration;

        public async Task<Stream> GetStream()
        {
            return await Task.Run(() =>
            {
                configuration = provider.GetConfiguration(new ConfigurationFileStreamValidator());
                return new FileStream(configuration.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 16384, useAsync: true);
            });
        }

    }
}
