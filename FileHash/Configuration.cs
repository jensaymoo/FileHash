using FluentValidation;

namespace FileHash
{
    internal class Configuration
    {
        public string FileName { get; set; }
        public int? BatchSize { get; set; }
        public int? TaskLimit { get; set; }
        public int? ChannelCapacity { get; set; }
    }

    internal class ConfigurationValidator : AbstractValidator<Configuration>
    {
        public ConfigurationValidator()
        {
            RuleFor(opt => opt.FileName)
                .NotNull()
                .NotEmpty();

            When(x => x.BatchSize is not null, () => {
                RuleFor(opt => opt.BatchSize)
                    .InclusiveBetween(4096, 4194304);
            });

            When(x => x.TaskLimit is not null, () => {
                RuleFor(opt => opt.TaskLimit)
                    .InclusiveBetween(1, 16);
            });

            When(x => x.ChannelCapacity is not null, () => {
                RuleFor(opt => opt.ChannelCapacity)
                    .InclusiveBetween(25, 150);
            });
        }
    }
}
