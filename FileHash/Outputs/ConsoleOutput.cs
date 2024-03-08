using Konsole;
namespace FileHash.Outputs
{
    internal class ConsoleOutput : IOutputProvider
    {
        ConcurrentWriter content;
        ConcurrentWriter info;
        ProgressBar progressBar;

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

            progressBar = new ProgressBar(console_rows[2], 100);

            info.WriteLine("Press CTRL + Q to abort");
        }

        public async Task PublishHash(byte[] hash)
        {
            await Task.Run(() =>
            {
                content.WriteLine(Convert.ToHexString(hash));

                lock (progressBar)
                {
                    progressBar.Next("Calculated hashes");
                }
            });
        }

        public async Task SetMaxBatchCount(int maxProggress)
        {
            progressBar.Max = maxProggress;
        }
    }
}
