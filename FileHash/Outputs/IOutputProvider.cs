namespace FileHash.Outputs;

internal interface IOutputProvider
{
    Task PublishHash(CancellationToken ct, byte[] hash);
    Task DisplayHashes(CancellationToken cts, int maxCount);
}