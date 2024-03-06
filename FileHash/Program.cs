using FileHash.Inputs;
using FileHash.Outputs;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace FileHash
{
    internal class Program : IProgram
    {
        IConfigProvider configProvider;
        IInput inputProvider;
        IOutput outputProvider;

        Configuration configuration;
        public Program(IConfigProvider configProvider, IInput inputProvider, IOutput outputProvider) 
        {
            this.configProvider = configProvider;
            this.inputProvider = inputProvider;
            this.outputProvider = outputProvider;

            configuration = this.configProvider.GetConfiguration(new BaseCommandLineValidator());
        }

        public async Task Run()
        {
            var abortTokenSource = new CancellationTokenSource();

            var channel = ReadInput(configuration.FileName, abortTokenSource, configuration.BatchSize, configuration.ChannelCapacity ?? 50);
            PublisHash(channel, abortTokenSource, configuration.TaskLimit ?? Environment.ProcessorCount);

            PeriodicTimer timer = new(TimeSpan.FromMilliseconds(50));
            while (await timer.WaitForNextTickAsync())
            {
                var key = await Task.Run(() => Console.ReadKey(true)); ;
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Q)
                {
                    await abortTokenSource.CancelAsync();
                    break;
                }
            }
        }

        private Channel<byte[]> ReadInput(string fileName, CancellationTokenSource cts, int batchSize = 4196, int capacityChannel = 50)
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
                    while ((buffer = await inputProvider.GetNextBatchBytesAsync()).Length != 0)
                    {
                        await channel.Writer.WriteAsync(buffer, ct);
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
        private void PublisHash(Channel<byte[]> channel, CancellationTokenSource cts, int taskLimit = 0)
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
                                    await outputProvider.Publish(sha256.ComputeHash(bytes));
                                };
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
