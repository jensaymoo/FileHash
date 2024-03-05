using Autofac;

namespace FileHash
{
    internal class MainEntry
    {

        public static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();
            ILifetimeScope scope;
            IProgram instance;

            try
            {
                builder.Register( c => new Commands.CommandLine(args))
                    .As<IConfigProvider>()
                    .InstancePerLifetimeScope();

                builder.RegisterType<Program>()
                    .As<IProgram>()
                    .InstancePerLifetimeScope();

                scope = builder.Build().BeginLifetimeScope();
                instance = scope.Resolve<IProgram>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }

            await instance.Run();


        }
    }
}
