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
			throw new InvalidOperationException($"Critical failure. Terraria steam directory '{this.programSettings.TerrariaSteamDir}' does not exist.");
		}

		await FindTerrariaDirectory(cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> SelectAndSetTerrariaDirectory(CancellationToken cancellationToken = default)
	{
		string? directory = await TrySelectTerrariaDirectory(cancellationToken).ConfigureAwait(false);
		if (!string.IsNullOrEmpty(directory)) {
			SetTerrariaDirectory(directory);

			return true;
		}

		return false;
	}

	private async Task<string?> TrySelectTerrariaDirectory(CancellationToken cancellationToken = default)
	{
		while (true) {
			string? executablePath = await terrariaExecutableSelectionPrompt.Prompt(cancellationToken).ConfigureAwait(false);

			if (executablePath == null) {
				return null;
			}

			string? errorText = null;

			if (Path.GetFileName(executablePath) != "Terraria.exe") {
				errorText = "File must be named Terraria.exe";
			}
			else if (!File.Exists(Path.Combine(Path.GetDirectoryName(executablePath)!, "TerrariaServer.exe"))) {
				errorText = "TerrariaServer.exe does not exist in the same directory";
			}

			if (errorText != null) {
				if (!userPrompt.Prompt(
					    "Invalid Selection",
					    errorText,
					    PromptOptions.RetryCancel)) {
					return null;
				}
			}
			else {
				return Path.GetDirectoryName(executablePath);
			}
		}
	}

	private async Task FindTerrariaDirectory(CancellationToken cancellationToken = default)
	{
		if (!SteamUtils.TryFindTerrariaDirectory(out string? terrariaFolderPath)) {
			const string messageText = "Unable to automatically find Terraria's installation path. Please select it manually.";

			bool continueSelection = userPrompt.Prompt("Error", messageText, PromptOptions.OKCancel, PromptSeverity.Error);
			if (continueSelection) {
				terrariaFolderPath = await this.TrySelectTerrariaDirectory(cancellationToken).ConfigureAwait(false);
			}

			if (!continueSelection || string.IsNullOrEmpty(terrariaFolderPath)) {
				Console.WriteLine("User chose to not retry. Exiting.");
				Environment.Exit(-1);
			}
		}

		this.SetTerrariaDirectory(terrariaFolderPath);
	}

	private void SetTerrariaDirectory(string path)
	{
		programSettings.TerrariaSteamDir = path;
		programSettings.TMLDevSteamDir = string.Empty;
		programSettings.Save();

		targetsFilesUpdater.Update();
	}
}