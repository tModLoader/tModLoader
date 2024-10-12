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
		string appSettingsPath,
		WorkspaceInfo workspaceInfo)
	{
		ProgramSettings programSettings = new(appSettingsPath);
		configuration.Bind(programSettings);

		TargetsFilesUpdater.Listen(workspaceInfo);

		return serviceCollection
			.AddSingleton<TerrariaExecutableSetter>()
			.AddSingleton(workspaceInfo)
			.AddSingleton(programSettings)
			.AddSingleton<TerrariaDecompileExecutableProvider>();
	}
}