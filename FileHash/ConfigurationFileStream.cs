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
                    .Must(x => {
                        try
                        {
                            using (var file = File.OpenRead(x))
                            {
                                return file.CanRead;
                            }
                        }
                        catch (Exception ex)
                        {
                            return false;
                        }
                    }).WithMessage("Заданный файл неудается прочитать.");
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
