using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.Core;

public class TerrariaExecutableSetter
{
	private readonly ProgramSettings programSettings;
	private readonly WorkspaceInfo workspaceInfo;
	private readonly ITerrariaExecutableSelectionPrompt terrariaExecutableSelectionPrompt;
	private readonly IUserPrompt userPrompt;

	public TerrariaExecutableSetter(
		ITerrariaExecutableSelectionPrompt terrariaExecutableSelectionPrompt,
		IUserPrompt userPrompt,
		ProgramSettings programSettings,
		WorkspaceInfo workspaceInfo)
	{
		this.terrariaExecutableSelectionPrompt = terrariaExecutableSelectionPrompt;
		this.userPrompt = userPrompt;
		this.programSettings = programSettings;
		this.workspaceInfo = workspaceInfo;
	}

	public async Task FindAndSetTerrariaDirectoryIfNecessary(
		string? terrariaSteamDirectoryOverride,
		string? tmlDevSteamDirectoryOverride,
		bool validateTerrariaDirectory,
		CancellationToken cancellationToken = default)
	{
		string terrariaDirectory = terrariaSteamDirectoryOverride ?? workspaceInfo.TerrariaSteamDirectory;

		if (!validateTerrariaDirectory) {
			SetTerrariaDirectory(terrariaDirectory, tmlDevSteamDirectoryOverride);
			return;
		}

		string[] fileNames = ["Terraria.exe", "TerrariaServer.exe"];
		string[] missingFiles = fileNames.Where(path => !File.Exists(Path.Combine(terrariaDirectory, path))).ToArray();

		if (missingFiles.Length == 0) {
			SetTerrariaDirectory(terrariaDirectory, tmlDevSteamDirectoryOverride);
			return;
		}

		if (!string.IsNullOrWhiteSpace(terrariaSteamDirectoryOverride)) {
			throw new InvalidOperationException($"Directory '{terrariaSteamDirectoryOverride}' does not contain: {string.Join(", ", missingFiles)}.");
		}

		await FindTerrariaDirectory(tmlDevSteamDirectoryOverride, cancellationToken);
	}

	public async Task<string> CheckTerrariaExecutablePathsAndPromptIfNecessary(CancellationToken cancellationToken = default)
	{
		string[] paths = [workspaceInfo.TerrariaPath, workspaceInfo.TerrariaServerPath];
		string[] missingFiles = paths.Where(path => !File.Exists(path)).ToArray();

		if (missingFiles.Length == 0)
			return workspaceInfo.TerrariaPath;

		if (programSettings.NoPrompts)
			throw new InvalidOperationException($"Missing required files: {string.Join(Environment.NewLine, missingFiles)}");

		userPrompt.Inform("Missing required files", string.Join(Environment.NewLine, missingFiles), PromptSeverity.Error);
		await SelectAndSetTerrariaDirectory(cancellationToken);

		return workspaceInfo.TerrariaPath;
	}

	public async Task SelectAndSetTerrariaDirectory(CancellationToken cancellationToken = default)
	{
		SetTerrariaDirectory(await PromptForTerrariaDirectory(cancellationToken), null);
	}

	private async Task<string> PromptForTerrariaDirectory(CancellationToken cancellationToken = default)
	{
		while (true) {
			string executablePath = await terrariaExecutableSelectionPrompt.Prompt(cancellationToken);

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

	private async Task FindTerrariaDirectory(string? tmlDevSteamDirectoryOverride, CancellationToken cancellationToken = default)
	{
		if (!SteamUtils.TryFindTerrariaDirectory(out string? terrariaFolderPath)) {
			const string messageText = "Unable to automatically find Terraria's installation path. Please select it manually.";

			if (programSettings.NoPrompts) {
				throw new InvalidOperationException(messageText);
			}

			userPrompt.Inform("Error", messageText, PromptSeverity.Error);

			terrariaFolderPath = await PromptForTerrariaDirectory(cancellationToken);
		}

		SetTerrariaDirectory(terrariaFolderPath, tmlDevSteamDirectoryOverride);
	}

	private void SetTerrariaDirectory(string terrariaSteamDirectory, string? tmlDevSteamDirectoryOverride)
	{
		workspaceInfo.UpdatePaths(terrariaSteamDirectory, tmlDevSteamDirectoryOverride ?? workspaceInfo.TMLDevSteamDirectory);
	}
}