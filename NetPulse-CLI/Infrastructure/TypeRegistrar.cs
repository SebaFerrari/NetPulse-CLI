using Spectre.Console.Cli;
using Microsoft.Extensions.DependencyInjection;

namespace NetPulse_CLI.Infrastructure
{
    public sealed class TypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceCollection _builder;

        public TypeRegistrar(IServiceCollection builder) => _builder = builder;

        public ITypeResolver Build() => new TypeResolver(_builder.BuildServiceProvider());

        public void Register(Type service, Type implementation)
            => _builder.AddSingleton(service, implementation);

        public void RegisterInstance(Type service, object implementation)
            => _builder.AddSingleton(service, implementation);

        public void RegisterLazy(Type service, Func<object> func)
        {
            ArgumentNullException.ThrowIfNull(func);
            _builder.AddSingleton(service, _ => func());
        }
    }
    public sealed class TypeResolver : ITypeResolver, IDisposable
    {
        private readonly IServiceProvider _provider;

        public TypeResolver(IServiceProvider provider)
            => _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        public object? Resolve(Type? type) => type is null ? null : _provider.GetService(type);

        public void Dispose()
        {
            if (_provider is IDisposable disposable) disposable.Dispose();
        }
    }
}
