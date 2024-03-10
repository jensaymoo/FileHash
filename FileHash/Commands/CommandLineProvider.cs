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
                    .MapIf(x => x.FileName, y => y["input"] is not null, v => v["input"]!)

                    .MapIf(x => x.BatchSize, y => y["batch"] is not null, v => v["batch"]!)
                    .MapIf(x => x.BatchSize, y => y["batch"] is null, v => 4096)

                    .MapIf(x => x.TaskLimit, y => y["task_limit"] is not null, v => v["task_limit"]!)
                    .MapIf(x => x.TaskLimit, y => y["task_limit"] is null, v => 16)

                    .MapIf(x => x.ChannelCapacity, y => y["channel_capacity"] is not null, v => v["channel_capacity"]!)
                    .MapIf(x => x.ChannelCapacity, y => y["channel_capacity"] is null, v => 64);


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
