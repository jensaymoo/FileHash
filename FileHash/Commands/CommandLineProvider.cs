using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace FileHash.Commands
{
    internal class CommandLineProvider : IConfigProvider
    {
        private string[] arguments;

        IMapper mapper;
        IConfigurationRoot configuration;

        public CommandLineProvider(string[] args)
        {
            var switchMappings = new Dictionary<string, string>()
            {
               { "-i", "input" },
               { "-b", "batch" },
               { "-t", "task_limit" },
               { "-c", "channel_capacity" },
            };

            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args, switchMappings);

            configuration = builder.Build();
            arguments = args;

            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IConfigurationRoot, ConfigurationFileStream>()
                    .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src["input"] ?? string.Empty))
                    .ForMember(dest => dest.BatchSize, opt => opt.MapFrom(src => src["batch"] ?? "4096"))
                    .ForMember(dest => dest.TaskLimit, opt => opt.MapFrom(src => src["task_limit"] ?? "16"))
                    .ForMember(dest => dest.ChannelCapacity, opt => opt.MapFrom(src => src["channel_capacity"] ?? "64"));

            }).CreateMapper();
        }

        public string GetConfigurationDescription()
        {
            return "-i - путь для входного файла (параметр обязательный)." + Environment.NewLine +
                   "-b - размер сегмента в байтах, по умолчанию 4096." + Environment.NewLine +
                   "-t - количество потоков для расчета хешей сегментов, по умочанию 16." + Environment.NewLine +
                   "-c - размер буфера в сегментах, по умочанию 64.";
        }

        public T GetConfiguration<T>(AbstractValidator<T>? validator = null) where T : new()
        {
            var value = mapper.Map<T>(configuration);

            if (validator != null)
                validator.ValidateAndThrow(value);

            return value;
        }

    }
}
