namespace FileHash
{
    internal class Program : IProgram
    {
        IConfigProvider commandsProvider;

        public Program(IConfigProvider provider) 
        {
            commandsProvider = provider;

            //try
            //{
            //    configValues = commandsProvider.GetConfiguration(new BaseCommandLineValidator());
            //}
            //catch 
            //{
            //    throw new Exception("Problem on validation for input params configuration.");
            //}

        }
        public async Task Run()
        {
            await Task.Run(() => Console.WriteLine("Hello, World!"));
        }
    }
}
