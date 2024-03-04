using FluentValidation;

namespace FileHash
{
    internal class Configuration
    {
        public string FileName { get; set; }
        public int Offset { get; set; }
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

            RuleFor(opt => opt.Offset)
                .NotNull()
                .NotEmpty();
        }
    }
}
