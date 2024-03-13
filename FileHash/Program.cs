using FileHash.Inputs;
using FileHash.Outputs;
using FluentValidation;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace FileHash
{
    internal class Program(IConfigurationProvider configProvider, IInputProvider inputProvider, IOutputProvider outputProvider) : IProgram
    {
        ConfigurationFileStream? configuration;

        public async Task Run()
        {
            var abortTokenSource = new CancellationTokenSource();
            var mainTask = Task.Run(async () =>
            {
                try
                {
                    using (var stream = await inputProvider.GetStream())
                    {
                        configuration = configProvider.GetConfiguration(new ConfigurationFileStreamValidator());

                        var channel = ReadInput(stream, abortTokenSource, configuration.BatchSize, configuration.ChannelCapacity);
                        _ = PublishHash(channel, abortTokenSource, configuration.TaskLimit);

                        var maxBatches = (int)Math.Ceiling((double)stream.Length / configuration.BatchSize);
                        await outputProvider.DisplayHashes(abortTokenSource.Token, maxBatches);

                        await abortTokenSource.CancelAsync();
                    }
                }
                catch (ValidationException ex)
                {
                    Console.WriteLine($"{Assembly.GetExecutingAssembly()?.GetName().Name} {Assembly.GetExecutingAssembly()?.GetName()?.Version.ToString()}" + Environment.NewLine);
                    Console.WriteLine(configProvider.GetConfigurationDescription() + Environment.NewLine);
                    
                    Console.WriteLine(ex.Message);
                    await abortTokenSource.CancelAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await abortTokenSource.CancelAsync();
                }
            });
            
            try
            {
                PeriodicTimer timer = new(TimeSpan.FromMilliseconds(15));
                while (await timer.WaitForNextTickAsync(abortTokenSource.Token))
                {
                    await Task.Run(async () =>
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey();

                            if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Q)
                            {
                                await abortTokenSource.CancelAsync();
                                return;
                            }
                        }
                    }, abortTokenSource.Token);
                }
            }
            catch (OperationCanceledException) { }

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
                    using (var sha256 = SHA256.Create())
                    {
                        try
                        {
                            while (await channel.Reader.WaitToReadAsync(ct))
                            {
                                while (channel.Reader.TryRead(out var bytes))
                                {
                                    await outputProvider.PublishHash(ct, sha256.ComputeHash(bytes));
                                }
                            }
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            await cts.CancelAsync();
                            throw;
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);        
        }
    }
}
