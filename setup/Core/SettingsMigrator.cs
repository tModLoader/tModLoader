using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using DiffPatch;

namespace Terraria.ModLoader.Setup.Core;

public static class SettingsMigrator
{
	public static void MigrateSettings(ProgramSettings programSettings, WorkspaceInfo workspaceInfo)
	{
		string settingsRootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Terraria");

		if (!Directory.Exists(settingsRootFolder)) {
			return;
		}

		var settingsFileInfos = Directory.EnumerateDirectories(settingsRootFolder, "setup*_Url_*", SearchOption.TopDirectoryOnly)
			.Select(x => new DirectoryInfo(x))
			.SelectMany(x => x.GetFiles("user.config", SearchOption.AllDirectories))
			.ToArray();

		var latestFile = settingsFileInfos.MaxBy(x => x?.LastWriteTimeUtc);

		if (latestFile == null) {
			return;
		}

		XDocument document = XDocument.Load(latestFile.FullName);

		MigrateDateTimes(document, programSettings);
		MigratePatchMode(document, programSettings);
		MigrateFormatAfterDecompiling(document, programSettings);
		programSettings.Save();

		MigrateWorkspaceInfo(document, workspaceInfo);

		CleanupDirectories(settingsFileInfos, settingsRootFolder);
	}

	private static void MigrateDateTimes(XDocument document, ProgramSettings programSettings)
	{
		Migration<DateTime>[] dateTimeMigrations = [
			new(x => programSettings.TerrariaDiffCutoff = x, "TerrariaDiffCutoff"),
			new(x => programSettings.TerrariaNetCoreDiffCutoff = x, "TerrariaNetCoreDiffCutoff"),
			new(x => programSettings.TModLoaderDiffCutoff = x, "tModLoaderDiffCutoff"),
		];

		foreach (Migration<DateTime> migration in dateTimeMigrations) {
			XElement? element = GetElement(document, migration.SettingName);
			if (element != null) {
				migration.UpdateAction(DateTime.Parse(element.Value, CultureInfo.InvariantCulture));
			}
		}
	}

	private static void MigratePatchMode(XDocument document, ProgramSettings programSettings)
	{
		XElement? element = GetElement(document, "PatchMode");
		if (element != null) {
			programSettings.PatchMode = (Patcher.Mode)int.Parse(element.Value);
		}
	}

	private static void MigrateFormatAfterDecompiling(XDocument document, ProgramSettings programSettings)
	{
		XElement? element = GetElement(document, "FormatAfterDecompiling");
		if (element != null) {
			programSettings.FormatAfterDecompiling = bool.Parse(element.Value);
		}
	}

	private static XElement? GetElement(XDocument document, string settingName)
	{
		return document.XPathSelectElement($"//setting[@name='{settingName}']/value");
	}

	private static void CleanupDirectories(IEnumerable<FileInfo> obsoleteSettingsFiles, string settingsRootFolder)
	{
		try {
			foreach (var fileInfo in obsoleteSettingsFiles) {
				Directory.Delete(Path.GetDirectoryName(fileInfo.DirectoryName!)!, true);
			}

			if (!Directory.EnumerateFileSystemEntries(settingsRootFolder).Any()) {
				Directory.Delete(settingsRootFolder);
			}
		}
		catch { }
	}

	private static void MigrateWorkspaceInfo(XDocument document, WorkspaceInfo workspaceInfo)
	{
		string? terrariaSteamDirectory = GetElement(document, "TerrariaSteamDir")?.Value;

		if (terrariaSteamDirectory != null) {
			workspaceInfo.UpdatePaths(
				terrariaSteamDirectory,
				GetElement(document, "TMLDevSteamDir")?.Value);
		}
	}

	private sealed record Migration<T>(Action<T> UpdateAction, string SettingName);
}