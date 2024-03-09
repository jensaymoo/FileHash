using FileHash.Inputs;
using FileHash.Outputs;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace FileHash
{
    internal class Program(IConfigProvider configProvider, IInputProvider inputProvider, IOutputProvider outputProvider) : IProgram
    {
        ConfigurationFileStream? configuration;

        public async Task Run()
        {
            try
            {
                using (var stream = await inputProvider.GetStream())
                { 
                    configuration = configProvider.GetConfiguration(new ConfigurationFileStreamValidator());
                    var maxBatches = (int)Math.Ceiling((double)stream.Length / configuration.BatchSize);
                    await outputProvider.SetMaxBatchCount(maxBatches);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            var abortTokenSource = new CancellationTokenSource();
            var mainTask = Task.Run(async () =>
            {
                try
                {
                    using (var stream = await inputProvider.GetStream())
                    {
                        var channel = ReadInput(stream, abortTokenSource, configuration.BatchSize, configuration.ChannelCapacity);
                        await PublishHash(channel, abortTokenSource, configuration.TaskLimit);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            });

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Q)
                {
                    await abortTokenSource.CancelAsync();
                    break;
                }
            }
        }

        private Channel<byte[]> ReadInput(Stream stream, CancellationTokenSource cts, int batchSize, int capacityChannel)
        {
            if (capacityChannel <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacityChannel));

            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize));

            CancellationToken ct = cts.Token;
            Channel<byte[]> channel = Channel.CreateBounded<byte[]>(capacityChannel);

            var readTask = Task.Run(async () =>
            {
                try
                {
                    byte[] buffer = new byte[batchSize];
                    while ((buffer = await stream.ReadBatchAsync(batchSize)).Length != 0)
                    {
                        await channel.Writer.WriteAsync(buffer, ct);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    cts.Cancel();
                    throw;
                }
                finally
                {
                    channel.Writer.Complete();
                }
            });

            return channel;
        }
        private async Task PublishHash(Channel<byte[]> channel, CancellationTokenSource cts, int taskLimit)
        {
            if (taskLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(taskLimit));

            CancellationToken ct = cts.Token;

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < taskLimit; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        while (await channel.Reader.WaitToReadAsync(ct))
                        {
                            while (channel.Reader.TryRead(out var bytes))
                            {
                                using (SHA256 sha256 = SHA256.Create())
                                {
                                    await outputProvider.PublishHash(sha256.ComputeHash(bytes));
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        cts.Cancel();
                        throw;
                    }
                }));
            }

            await Task.WhenAll(tasks);        
        }
    }
}
