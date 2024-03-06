namespace FileHash.Outputs
{
    internal interface IOutput
    {
        Task Publish(byte[] hash);
    }
}
