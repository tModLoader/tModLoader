using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Setup.GUI.Avalonia.Services;
using Setup.GUI.Avalonia.ViewModels;
using Setup.GUI.Avalonia.Views;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Setup.GUI.Avalonia;

internal static class Bootstrapper
{
	public static IServiceProvider Initialize(MainWindow mainWindow)
	{
		string userSettingsFilePath = Path.Combine("setup", "user.settings");
		WorkspaceInfo workspaceInfo = WorkspaceInfo.Initialize();
		if (!File.Exists(userSettingsFilePath)) {
			ProgramSettings programSettings = ProgramSettings.InitializeSettingsFile(userSettingsFilePath);
			SettingsMigrator.MigrateSettings(programSettings, workspaceInfo);
		}

		IConfigurationRoot configuration = new ConfigurationBuilder()
			.AddJsonFile(Path.Combine(Environment.CurrentDirectory, userSettingsFilePath), false, true)
			.Build();

		IServiceCollection services = new ServiceCollection();
		services
			.AddCoreServices(configuration, userSettingsFilePath, workspaceInfo)
			.AddSingleton<ITerrariaExecutableSelectionPrompt, TerrariaExecutableSelectionPrompt>()
			.AddSingleton<IUserPrompt, UserPrompt>()
			.AddSingleton<MainWindowViewModel>()
			.AddSingleton<IFilesService>(_ => new FilesService(mainWindow.StorageProvider));

#if WINDOWS
		services.AddSingleton<IPatchReviewer, PatchReviewer>();
#endif

		return services.BuildServiceProvider();
	}
}