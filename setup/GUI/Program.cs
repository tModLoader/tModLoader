using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using DiffPatch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.GUI
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

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
				.AddSingleton<MainForm>()
				.AddSingleton<IPatchReviewer>(sp => sp.GetRequiredService<MainForm>());

			IServiceProvider serviceProvider = services.BuildServiceProvider();

			workspaceInfo.UpdateGitInfo();

			Application.Run(serviceProvider.GetRequiredService<MainForm>());
		}
	}
}