using Autofac;
using FileHash.Commands;
using FileHash.Inputs;
using FileHash.Outputs;
using FluentValidation;

namespace FileHash;

internal class MainEntry
{

    public static async Task Main(string[] args)
    {
        var builder = new ContainerBuilder();
        ILifetimeScope scope;
        IProgram instance;

        try
        {
            builder.Register(c => new CommandLineProvider(args))
                .As<IConfigurationProvider>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ConfigurationFileStreamValidator>()
                .As<IValidator<ConfigurationFileStream>>()
                .InstancePerLifetimeScope();

            builder.RegisterType<FileStreamInput>()
                .As<IInputProvider>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ConsoleOutput>()
                .As<IOutputProvider>()
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