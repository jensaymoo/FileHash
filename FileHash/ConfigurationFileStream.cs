using FluentValidation;

namespace FileHash
{
    internal class ConfigurationFileStream
    {
        public string FileName { get; set; }
        public int BatchSize { get; set; }
        public int TaskLimit { get; set; }
        public int ChannelCapacity { get; set; }
    }

    internal class ConfigurationFileStreamValidator : AbstractValidator<ConfigurationFileStream>
    {
        public ConfigurationFileStreamValidator()
        {
            RuleFor(opt => opt.FileName).Cascade(CascadeMode.Stop)
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


            RuleFor(opt => opt.BatchSize).Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty()
                .InclusiveBetween(4096, 4194304);

            RuleFor(opt => opt.TaskLimit).Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty()
                .InclusiveBetween(1, 64);

            RuleFor(opt => opt.ChannelCapacity).Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty()
                .InclusiveBetween(64, 254);
        }
    }
}
