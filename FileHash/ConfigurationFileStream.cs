using FluentValidation;

namespace FileHash
{
    internal class ConfigurationFileStream
    {
        public string FileName { get; set; }
        public int? BatchSize { get; set; }
        public int? TaskLimit { get; set; }
        public int? ChannelCapacity { get; set; }
    }

    internal class ConfigurationFileStreamValidator : AbstractValidator<ConfigurationFileStream>
    {
        public ConfigurationFileStreamValidator()
        {
            RuleFor(opt => opt.FileName)
                .NotNull()
                .NotEmpty()
                .Must(x => File.Exists(x)).WithMessage("Заданный файл не найден.");

            When(x => File.Exists(x.FileName), () =>
            {
                RuleFor(opt => opt.FileName)
                    .Custom((val, context) =>
                    {
                        try
                        {
                            using (var file = File.OpenRead(val))
                            {
                                if (!file.CanRead)
                                    context.AddFailure("Заданный файл не удается прочитать по неизвестной причине.");
                            }
                        }
                        catch (Exception ex)
                        {
                            context.AddFailure($"Заданный файл не удается прочитать: {ex.Message}");
                        }
                    });
            });


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
