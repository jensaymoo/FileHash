namespace FileHash.Outputs
{
    internal interface IOutputProvider
    {
        Task PublishHash(byte[] hash);
        Task SetMaxBatchCount(int maxProggress);
    }
}
