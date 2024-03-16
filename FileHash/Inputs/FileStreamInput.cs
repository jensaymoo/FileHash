using FluentValidation;

namespace FileHash.Inputs;

internal class FileStreamInput(IConfigurationProvider configProvider, IValidator<ConfigurationFileStream> configValidator) : IInputProvider
{
    ConfigurationFileStream? configuration;

    public async Task<Stream> GetStream()
    {
        return await Task.Run(() =>
        {
            configuration = configProvider.GetConfiguration(configValidator);
            return new FileStream(configuration.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 16384, useAsync: true);
        });
    }

}