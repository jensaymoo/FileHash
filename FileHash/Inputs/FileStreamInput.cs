namespace FileHash.Inputs
{
    internal class FileStreamInput(IConfigProvider provider) : IInputProvider
    {
        ConfigurationFileStream? configuration;

        public async Task<Stream> GetStream()
        {
            try
            {
                configuration = provider.GetConfiguration(new ConfigurationFileStreamValidator());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Configuration validation failed", ex);
            }

            return new FileStream(configuration.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 16384, useAsync: true);
        }

    }
}
