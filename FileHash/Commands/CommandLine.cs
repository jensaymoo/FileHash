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
            mapper = new MapperConfiguration(cfg =>cfg.CreateMap<BaseConfigCommands, Configuration>())
                .CreateMapper();
        }

        public T GetConfiguration<T>(AbstractValidator<T>? validator = null) where T : new()
        {
            var commands = Parser.Default.ParseArguments<BaseConfigCommands>(arguments).Value;
            var value = mapper.Map<T>(commands);

            if (validator != null)
                validator.ValidateAndThrow(value);

            return value;
        }
    }
}
