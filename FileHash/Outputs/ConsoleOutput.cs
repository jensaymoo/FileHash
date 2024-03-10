using Spectre.Console;
using System.Collections.Concurrent;

namespace FileHash.Outputs
{
    internal class ConsoleOutput : IOutputProvider
    {
        ConcurrentQueue<string> outputQueue = new ConcurrentQueue<string>();

        public async Task PublishHash(byte[] hash)
        {
            outputQueue.Enqueue(Convert.ToHexString(hash));
        }

        public async Task DisplayHashes(int maxCount)
        {
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var progressTask = ctx.AddTask("Reciving hashes", maxValue: maxCount);

                    while (!ctx.IsFinished)
                    {
                        while (outputQueue.Count > 0)
                        {
                            while (outputQueue.TryDequeue(out var result))
                            {
                                progressTask.Increment(1);
                                AnsiConsole.WriteLine(result);
                            }
                        }
                    }
                    progressTask.Value = progressTask.MaxValue;
                });
        }
    }
}
