// See https://aka.ms/new-console-template for more information

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.CLI;
using Terraria.ModLoader.Setup.CLI.Commands;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;

public static class Program
{
	public static async Task<int> Main(string[] args)
	{
		CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

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
			.AddSingleton<TaskRunner>();

		var app = new CommandApp(new TypeRegistrar(services));
		app.Configure(config => {
#if DEBUG
			config.PropagateExceptions();
			config.ValidateExamples();
#endif

			config.AddCommand<SetupCommand>("setup")
				.WithDescription("Complete environment setup. Equivalent to Decompile + Regen Source");

			config.AddCommand<DecompileCommand>("decompile")
				.WithDescription("Decompiles Terraria. Outputs to src/decompiled.");

			config.AddBranch("diff", x => {
				x.SetDescription("Diffs the source to recalculate patches.");

				x.AddCommand<DiffTerrariaCommand>("terraria")
					.WithDescription("Diffs src/Terraria against src/decompiled");
				x.AddCommand<DiffTerrariaNetCoreCommand>("netcore")
					.WithDescription("Diffs src/TerrariaNetCore against src/Terraria");
				x.AddCommand<DiffTModLoaderCommand>("tml")
					.WithDescription("Diffs src/tModLoader against src/TerrariaNetCore. Use this after making changes and then commit the patches to git.");
			});

			config.AddBranch("patch", x => {
				x.SetDescription("Applies patches.");

				x.AddCommand<PatchTerrariaCommand>("terraria")
					.WithDescription("Patches source in src/Terraria");
				x.AddCommand<PatchTerrariaNetCoreCommand>("netcore")
					.WithDescription("Patches source in src/TerrariaNetCore");
				x.AddCommand<PatchTModLoaderCommand>("tml")
					.WithDescription("Patches source in src/tModLoader. Edit the source code in src/tModLoader after this phase.");
			});

			config.AddCommand<RegenSourceCommand>("regen-source")
				.WithDescription("Regenerates all source files. Use this after pulling. Equivalent to Setup without decompile.");

			config.AddCommand<FormatCommand>("format")
				.WithDescription("Formats source files for a given .csproj file.");

			config.AddCommand<HookGenCommand>("hook-gen")
				.WithDescription("Generates TerrariaHooks.dll.");

			config.AddCommand<SimplifyCommand>("simplify")
				.WithDescription("Uses Microsoft.CodeAnalysis.Simplification.Simplifier to reduce code in a project.");

			config.AddCommand<UpdateLocalizationFilesCommand>("update-localization-files")
				.WithDescription("Updates other localization files after adding new keys to en-US.tModLoader.json. Requires python 3.");

			config.AddBranch("secret", x => {
				x.AddCommand<SecretEncryptCommand>("encrypt");
				x.AddCommand<SecretOwnershipCommand>("ownership");
				x.AddCommand<RevealKeyCommand>("reveal");
			});

			config.AddCommand<UpdateWorkspaceInfoCommand>("update-workspace-info")
				.WithDescription("Updates the paths in WorkspaceInfo.targets");
		});

		return await app.RunAsync(args);
	}
}