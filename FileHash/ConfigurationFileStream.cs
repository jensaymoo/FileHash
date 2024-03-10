using FluentValidation;

namespace FileHash
{
    internal class ConfigurationFileStream
    {
        public string FileName { get; set; }
        public int BatchSize { get; set; } = 4096;
        public int TaskLimit { get; set; } = 16;
        public int ChannelCapacity { get; set; } = 150;
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


            RuleFor(opt => opt.BatchSize)
                .NotNull()
                .NotEmpty()
                .InclusiveBetween(4096, 4194304);

            RuleFor(opt => opt.TaskLimit)
                .NotNull()
                .NotEmpty()
                .InclusiveBetween(1, 64);

            RuleFor(opt => opt.ChannelCapacity)
                .NotNull()
                .NotEmpty()
                .InclusiveBetween(50, 250);
        }
    }
}
