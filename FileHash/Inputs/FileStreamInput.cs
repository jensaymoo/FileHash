using FluentValidation;

namespace FileHash.Inputs
{
    internal class FileStreamInput(IConfigurationProvider provider, IValidator<ConfigurationFileStream> validator) : IInputProvider
    {
        ConfigurationFileStream? configuration;

        public async Task<Stream> GetStream()
        {
            return await Task.Run(() =>
            {
                configuration = provider.GetConfiguration(validator);
                return new FileStream(configuration.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 16384, useAsync: true);
            });
        }

    }
}
