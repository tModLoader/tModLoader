using System.Threading;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.Core;

public class TerrariaExecutableSetter
{
	private readonly ProgramSettings programSettings;
	private readonly ITerrariaExecutableSelectionPrompt terrariaExecutableSelectionPrompt;
	private readonly IUserPrompt userPrompt;
	private readonly TargetsFilesUpdater targetsFilesUpdater;

	public TerrariaExecutableSetter(
		ITerrariaExecutableSelectionPrompt terrariaExecutableSelectionPrompt,
		IUserPrompt userPrompt,
		TargetsFilesUpdater targetsFilesUpdater,
		ProgramSettings programSettings)
	{
		this.terrariaExecutableSelectionPrompt = terrariaExecutableSelectionPrompt;
		this.userPrompt = userPrompt;
		this.targetsFilesUpdater = targetsFilesUpdater;
		this.programSettings = programSettings;
	}

	public async Task FindAndSetTerrariaDirectoryIfNecessary(CancellationToken cancellationToken = default)
	{
		if (Directory.Exists(programSettings.TerrariaSteamDir)) {
			return;
		}

		if (programSettings.NoPrompts) {
			throw new InvalidOperationException($"Critical failure. Terraria steam directory '{programSettings.TerrariaSteamDir}' does not exist.");
		}

		await FindTerrariaDirectory(cancellationToken).ConfigureAwait(false);
	}

	public async Task CheckTerrariaExecutablePathsAndPromptIfNecessary(CancellationToken cancellationToken = default)
	{
		string[] paths = [programSettings.TerrariaPath!, programSettings.TerrariaServerPath!];
		if (paths.All(File.Exists))
			return;

		var missing = paths.First(f => !File.Exists(f));
		if (programSettings.NoPrompts)
			throw new FileNotFoundException(missing);

		userPrompt.Inform("Missing required file", missing, PromptSeverity.Error);
		await SelectAndSetTerrariaDirectory(cancellationToken).ConfigureAwait(false);
	}

	public async Task SelectAndSetTerrariaDirectory(CancellationToken cancellationToken = default)
	{
		SetTerrariaDirectory(await PromptForTerrariaDirectory(cancellationToken).ConfigureAwait(false));
	}

	private async Task<string> PromptForTerrariaDirectory(CancellationToken cancellationToken = default)
	{
		while (true) {
			string executablePath = await terrariaExecutableSelectionPrompt.Prompt(cancellationToken).ConfigureAwait(false);

			string errorText;
			if (Path.GetFileName(executablePath) != "Terraria.exe") {
				errorText = "File must be named Terraria.exe";
			}
			else if (!File.Exists(Path.Combine(Path.GetDirectoryName(executablePath)!, "TerrariaServer.exe"))) {
				errorText = "TerrariaServer.exe does not exist in the same directory";
			}
			else {
				return Path.GetDirectoryName(executablePath)!;
			}

			if (!userPrompt.Prompt(
					"Invalid Selection",
					errorText,
					PromptOptions.RetryCancel)) {
				throw new OperationCanceledException();
			}
		}
	}

	private async Task FindTerrariaDirectory(CancellationToken cancellationToken = default)
	{
		if (!SteamUtils.TryFindTerrariaDirectory(out string? terrariaFolderPath)) {
			const string messageText = "Unable to automatically find Terraria's installation path. Please select it manually.";

			bool continueSelection = userPrompt.Prompt("Error", messageText, PromptOptions.OKCancel, PromptSeverity.Error);
			if (!continueSelection)
				throw new OperationCanceledException();

			terrariaFolderPath = await PromptForTerrariaDirectory(cancellationToken).ConfigureAwait(false);
		}

		SetTerrariaDirectory(terrariaFolderPath);
	}

	private void SetTerrariaDirectory(string path)
	{
		programSettings.TerrariaSteamDir = path;
		programSettings.TMLDevSteamDir = string.Empty;
		programSettings.Save();

		targetsFilesUpdater.Update();
	}
}