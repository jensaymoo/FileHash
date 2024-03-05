using FluentValidation;

namespace FileHash
{
    internal class Configuration
    {
        public string FileName { get; set; }
        public int BatchSize { get; set; }
        public int? TaskLimit { get; set; }
        public int? ChannelCapacity { get; set; }
    }

    internal class BaseCommandLineValidator : AbstractValidator<Configuration>
    {
        public BaseCommandLineValidator()
        {
            RuleFor(opt => opt.FileName)
                .NotNull()
                .NotEmpty()
                .Must(File.Exists)
                    .WithMessage("File not exists");

            RuleFor(opt => opt.BatchSize)
                .NotNull()
                .NotEmpty();
        }
    }
}
