namespace FileHash;

internal static class Extensions
{
    public static int ReadBatch(this Stream stream, ref byte[] output)
    {
        int numRead = stream.Read(output, 0, output.Length);
        if (numRead < output.Length)
            Array.Resize(ref output, numRead);

        return numRead;
    }
}