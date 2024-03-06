using Konsole;
namespace FileHash.Outputs
{
    internal class ConsoleOutput : IOutput
    {
        ConcurrentWriter content;
        ConcurrentWriter info;
        ConcurrentWriter progress;
        
        public ConsoleOutput()
        {
            var console = new Window();
            var console_rows = console.SplitRows(
                    new Split(0),
                    new Split(1),
                    new Split(1)
            );

            content = new Window(console_rows[0]).Concurrent();
            info = new Window(console_rows[1]).Concurrent();

            progress = new Window(console_rows[2]).Concurrent();

            info.WriteLine("Press CTRL + Q to abort");
        }

        object progressLocker = new object();
        int progress_status = 0;

        public async Task Publish(byte[] hash)
        {
            _ = Task.Run(() =>
            {
                lock (progressLocker)
                {
                    progress_status++;

                    progress.WriteLine($"Received batches count {progress_status}");
                    content.WriteLine(Convert.ToHexString(hash));
                }
            });
        }
    }
}
