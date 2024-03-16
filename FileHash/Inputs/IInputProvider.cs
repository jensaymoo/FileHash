namespace FileHash.Inputs;

internal interface IInputProvider
{
    Task<Stream> GetStream();
}