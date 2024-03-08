namespace FileHash.Inputs
{
    internal class FileStreamInput(IConfigProvider provider) : IInputProvider
    {
        Configuration? configuration;

        public async Task<Stream> GetStream()
        {
            try
            {
                configuration = provider.GetConfiguration(new ConfigurationValidator());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return new FileStream(configuration.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 16384, useAsync: true);
        }

    }
}
