using AutoMapper;
using CommandLine;
using FluentValidation;

namespace FileHash.Commands
{
    internal class CommandLineProvider : IConfigProvider
    {
        private string[] arguments;
        IMapper mapper;

        public CommandLineProvider(string[] args)
        {
            arguments = args;
            mapper = new MapperConfiguration(cfg => 
            {
                cfg.CreateMap<CommandLine, ConfigurationFileStream>()
                    .MapIf(x => x.FileName, y => y.Input is not null, v => v.Input!)
                    .MapIf(x => x.BatchSize, y => y.Batch is not null, v => v.Batch!)
                    .MapIf(x => x.TaskLimit, y => y.TaskLimit is not null, v => v.TaskLimit!)
                    .MapIf(x => x.ChannelCapacity, y => y.ChannelCapacity is not null, v => v.ChannelCapacity!);

            }).CreateMapper();
        }

        public T GetConfiguration<T>(AbstractValidator<T>? validator = null) where T : new()
        {
            var commands = Parser.Default.ParseArguments<CommandLine>(arguments).Value;
            var value = mapper.Map<T>(commands);

            if (validator != null)
                validator.ValidateAndThrow(value);

            return value;
        }

    }
}
