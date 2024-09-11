using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class UpdateWorkspaceInfoCommandSettings : CommandSettings
{
	private readonly string terrariaSteamDir;
	private readonly string? tmlDevSteamDir;

	[CommandArgument(0, "<TERRARIA_STEAM_DIR>")]
	[Description("The Terraria steam directory.")]
	public required string TerrariaSteamDir {
		get => terrariaSteamDir;
		[MemberNotNull(nameof(terrariaSteamDir))]
		init => terrariaSteamDir = PathUtils.GetCrossPlatformFullPath(value);
	}

	[CommandArgument(1, "[TML_DEV_STEAM_DIR]")]
	[Description("The TML dev steam directory. This is derived from the Terraria steam directory if no value is supplied.")]
	public string? TmlDevSteamDir {
		get => tmlDevSteamDir;
		init => tmlDevSteamDir = value != null ? PathUtils.GetCrossPlatformFullPath(value) : null;
	}

	[CommandOption("--no-validate")]
	[Description("Skip checking the Terraria steam directory for Terraria.exe")]
	public bool NoValidate { get; set; }

	public override ValidationResult Validate()
	{
		if (!Directory.Exists(TerrariaSteamDir)) {
			return ValidationResult.Error("The Terraria steam directory does not exist.");
		}

		return ValidationResult.Success();
	}
}

public sealed class UpdateWorkspaceInfoCommand : CancellableAsyncCommand<UpdateWorkspaceInfoCommandSettings>
{
	private readonly TerrariaExecutableSetter terrariaExecutableSetter;

	public UpdateWorkspaceInfoCommand(TerrariaExecutableSetter terrariaExecutableSetter)
	{
		this.terrariaExecutableSetter = terrariaExecutableSetter;
	}

	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		UpdateWorkspaceInfoCommandSettings settings,
		CancellationToken cancellationToken)
	{
		try {
			await terrariaExecutableSetter.FindAndSetTerrariaDirectoryIfNecessary(
				settings.TerrariaSteamDir,
				settings.TmlDevSteamDir,
				!settings.NoValidate,
				cancellationToken);

			return 0;
		}
		catch (Exception exception) {
			AnsiConsole.WriteException(exception);

			return 1;
		}
	}
}