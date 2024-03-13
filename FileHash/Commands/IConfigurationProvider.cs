using FluentValidation;

namespace FileHash
{
    internal interface IConfigurationProvider
    {
        public string GetConfigurationDescription();
        public T GetConfiguration<T>(IValidator<T>? validator = null) where T : class, new();
    }
}
