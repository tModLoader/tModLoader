using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Terraria.ModLoader.Setup.Core;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCoreServices(
		this IServiceCollection serviceCollection,
		IConfiguration configuration,
		string appSettingsPath)
	{
		ProgramSettings programSettings = new(appSettingsPath);
		configuration.Bind(programSettings);

		return serviceCollection
			.AddTransient<TerrariaExecutableSetter>()
			.AddTransient<TargetsFilesUpdater>()
			.AddSingleton(programSettings);
	}
}