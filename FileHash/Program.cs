using FileHash.Outputs;
using System.Security.Cryptography;
using System.Threading.Channels;
using Konsole.Internal;
using Konsole;
using System;
using System.Text;

namespace FileHash
{
    internal class Program : IProgram
    {
        IConfigProvider commandsProvider;

        Configuration configuration;
        public Program(IConfigProvider provider) 
        {
            commandsProvider = provider;
            configuration = commandsProvider.GetConfiguration(new BaseCommandLineValidator());
        }


        public async Task Run()
        {
            var con = new Window();
            var consoles = con.SplitRows(
                    new Split(0),
                    new Split(1)
            );

            var content = new Window(consoles[0]).Concurrent();
            var progress = new ProgressBar(consoles[1], Convert.ToInt32(new FileInfo(configuration.FileName).Length / configuration.BatchSize));

            var abortTokenSource = new CancellationTokenSource();

            var channel = ReadFile(configuration.FileName, content, abortTokenSource, configuration.BatchSize, configuration.ChannelCapacity ?? 50);
            PrintHashes(channel, content, progress, abortTokenSource, configuration.TaskLimit ?? Environment.ProcessorCount);


            PeriodicTimer timer = new(TimeSpan.FromMilliseconds(50));
            while (await timer.WaitForNextTickAsync())
            {
                var key = await WaitConsoleKey();
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Q)
                {
                    await abortTokenSource.CancelAsync();
                    break;
                }
            }

        }
        private static async Task<ConsoleKeyInfo> WaitConsoleKey()
        {
            ConsoleKeyInfo key = default;
            await Task.Run(() => key = Console.ReadKey(true));
            return key;
        }

        private Channel<byte[]> ReadFile(string fileName, IConsole console, CancellationTokenSource cts, int batchSize = 4196, int capacityChannel = 50)
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

                    using (FileStream sourceStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: batchSize, useAsync: true))
                    {
                        byte[] buffer = new byte[batchSize];
                        int numRead;

                        while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            var zippedBuffer = new byte[numRead];
                            Array.Copy(buffer, zippedBuffer, numRead);

                            await channel.Writer.WriteAsync(zippedBuffer, ct);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    console.WriteLine(ConsoleColor.Red, ex.ToString());
                    cts.Cancel();
                }
                finally
                {
                    channel.Writer.Complete();
                }
            });

            return channel;

        }
        private void PrintHashes(Channel<byte[]> channel, IConsole console, IProgressBar progressBar, CancellationTokenSource cts, int taskLimit = 0)
        {
            if (taskLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(taskLimit));

            CancellationToken ct = cts.Token;

            object statusLocker = new object();
            int status = 0;
            //таска для обновления прогрессбара
            async Task UpdateProgress() => await Task.Run(() =>
            {
                lock (statusLocker)
                {
                    status++;
                    progressBar.Refresh(status, "Calculating hashes...");
                }
            });

            //делаем массив длинной taskLimit чтобы ограничить колво одновременных таск
            var hashTasks = Enumerable.Range(1, taskLimit)
            .Select(x => Task.Run(async () =>
            {
                try
                {
                    while (await channel.Reader.WaitToReadAsync(ct))
                    {
                        ct.ThrowIfCancellationRequested();
                        while (channel.Reader.TryRead(out var bytes))
                        {
                            using (SHA256 sha256 = SHA256.Create())
                            {
                                console.WriteLine(Convert.ToHexString(sha256.ComputeHash(bytes)));
                                await UpdateProgress();
                            };
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch
                {
                    cts.Cancel();
                    throw;
                }
            }))
            .ToArray();

            ////Task waitForHash = Task.WhenAll(hashTasks);
            //try { await waitForHash; }
            //catch
            //{
            //    if (waitForHash.IsFaulted && waitForHash.Exception is not null)
            //    {
            //        foreach (var e in waitForHash.Exception.Flatten().InnerExceptions)
            //            console.WriteLine(ConsoleColor.Red, e.ToString());

            //        throw waitForHash.Exception.Flatten();
            //    }
            //    else throw;
            //}
        }

        public async Task ProcessMessages(string filename, IConsole console, IProgressBar progressBar, CancellationTokenSource cts, int batchSize, int capacityTask = 50, int taskLimit = 0)
        {
            //if (taskLimit <= 0)
            //    taskLimit = Environment.ProcessorCount;


            //var channel = await ReadFile(filename, console, cts);
            //await PrintHashesAsync(channel, console, progressBar, cts, taskLimit);
            //CancellationToken ct = cts.Token;

            //object statusLocker = new object();
            //int status = 0;
            ////таска для обновления прогрессбара
            //async Task UpdateProgress() => await Task.Run(() =>
            //{
            //    lock (statusLocker)
            //    {
            //        status++;
            //        progressBar.Refresh(status, filename);
            //    }
            //});

            ////делаем массив длинной taskLimit 
            //var hashTasks = Enumerable.Range(1, taskLimit)
            //.Select(x => Task.Run(async () =>
            //{
            //    try
            //    {
            //        while (await channel.Reader.WaitToReadAsync(ct))
            //        {
            //            ct.ThrowIfCancellationRequested();
            //            while (channel.Reader.TryRead(out var bytes))
            //            {
            //                using (SHA256 mySHA256 = SHA256.Create())
            //                {
            //                    byte[] hashValue = mySHA256.ComputeHash(bytes);

            //                    console.WriteLine(Convert.ToHexString(hashValue));
            //                    await UpdateProgress();
            //                }
            //            }
            //        }
            //    }
            //    catch (OperationCanceledException) { }
            //    catch
            //    {
            //        cts.Cancel();
            //        throw;
            //    }
            //}))
            //.ToArray();

            //Task waitForHash = Task.WhenAll(hashTasks);
            //try { await waitForHash; }
            //catch
            //{
            //    if (waitForHash.IsFaulted && waitForHash.Exception is not null)
            //    {
            //        foreach (var e in waitForHash.Exception.Flatten().InnerExceptions)
            //            console.WriteLine(e.ToString());

            //        throw waitForHash.Exception.Flatten();
            //    }
            //    else throw;
            //}

        }
    }
}
