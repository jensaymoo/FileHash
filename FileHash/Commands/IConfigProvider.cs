using FluentValidation;

namespace FileHash
{
    internal interface IConfigProvider
    {
        public T GetConfiguration<T>(AbstractValidator<T>? validator = null) where T : new();
    }
}
