using Spectre.Console.Cli;

namespace Terraria.ModLoader.Setup.CLI;

public sealed class TypeResolver : ITypeResolver, IDisposable
{
	private readonly IServiceProvider serviceProvider;

	public TypeResolver(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public object? Resolve(Type? type)
	{
		return type == null ? null : serviceProvider.GetService(type);
	}

	public void Dispose()
	{
		if (serviceProvider is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}