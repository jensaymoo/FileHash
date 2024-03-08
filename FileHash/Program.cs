using FileHash.Inputs;
using FileHash.Outputs;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace FileHash
{
    internal class Program(IConfigProvider configProvider, IInputProvider inputProvider, IOutputProvider outputProvider) : IProgram
    {
        const int defautBatchSize = 4096;
        const int defaultChannelCapacity = 50;

        Configuration? configuration;

        public async Task Run()
        {
            try
            {
                configuration = configProvider.GetConfiguration(new ConfigurationValidator());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }


            var abortTokenSource = new CancellationTokenSource();

            using (var stream = await inputProvider.GetStream())
            {
                var maxBathces = (int)Math.Ceiling((double)stream.Length / (configuration.BatchSize ?? defautBatchSize));
                await outputProvider.SetMaxBatchCount(maxBathces);
            }

            var channel = ReadInput(abortTokenSource, configuration.BatchSize ?? defautBatchSize, configuration.ChannelCapacity ?? defaultChannelCapacity);
            PublishHash(channel, abortTokenSource, configuration.TaskLimit ?? Environment.ProcessorCount);

            
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

        private Channel<byte[]> ReadInput(CancellationTokenSource cts, int batchSize, int capacityChannel)
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
                    using (var stream = await inputProvider.GetStream())
                    {
                        byte[] buffer = new byte[batchSize];
                        while ((buffer = await stream.ReadBatchAsync(batchSize)).Length != 0)
                        {
                            await channel.Writer.WriteAsync(buffer, ct);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    cts.Cancel();
                }
                finally
                {
                    channel.Writer.Complete();
                }
            });

            return channel;
        }
        private void PublishHash(Channel<byte[]> channel, CancellationTokenSource cts, int taskLimit = 0)
        {
            if (taskLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(taskLimit));

            CancellationToken ct = cts.Token;
            for (int i = 0; i < taskLimit; i++)
            {
                var publishTask = Task.Run(async () =>
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
                        Console.WriteLine(ex.ToString());
                        cts.Cancel();
                    }
                });
            }
        }
    }
}
