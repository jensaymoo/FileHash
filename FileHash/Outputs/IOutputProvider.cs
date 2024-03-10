namespace FileHash.Outputs
{
    internal interface IOutputProvider
    {
        Task PublishHash(byte[] hash);
        Task DisplayHashes(int maxCount);
    }
}
