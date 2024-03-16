using Spectre.Console;
using System.Collections.Concurrent;

namespace FileHash.Outputs;

internal class ConsoleOutput : IOutputProvider
{
    ConcurrentQueue<string> outputQueue = new ConcurrentQueue<string>();
    public async Task PublishHash(CancellationToken ct, byte[] hash)
    {
        outputQueue.Enqueue(Convert.ToHexString(hash));
    }

    public async Task DisplayHashes(CancellationToken ct, int maxCount)
    {
        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                await Task.Run(() =>
                {
                    var progressTask = ctx.AddTask("Published hashes", maxValue: maxCount);
                    try
                    {
                        while (!ctx.IsFinished)
                        {
                            while (outputQueue.Count > 0)
                            {
                                while (outputQueue.TryDequeue(out var result))
                                {
                                    ct.ThrowIfCancellationRequested();

                                    progressTask.Increment(1);
                                    AnsiConsole.WriteLine(result);
                                }
                            }
                        }
                        progressTask.Value = progressTask.MaxValue;
                    }
                    catch (OperationCanceledException) { }
                    finally
                    {
                        AnsiConsole.Reset();
                    }
                });
            });
    }
}