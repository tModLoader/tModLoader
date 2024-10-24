namespace Terraria.ModLoader.Setup.Core;

internal static class TargetsFilesUpdater
{
	public static void Listen(WorkspaceInfo workspaceInfo)
	{
		if (!string.IsNullOrWhiteSpace(workspaceInfo.TMLDevSteamDirectory)) {
			CopyTmlModTargets(workspaceInfo.TMLDevSteamDirectory);
		}

		workspaceInfo.OnTmlDevSteamDirectoryChanged(CopyTmlModTargets);
	}

	private static void CopyTmlModTargets(string tmlDevSteamDirectory)
	{
		string tMLModTargetsContents = File.ReadAllText("patches/tModLoader/Terraria/release_extras/tMLMod.targets");

		UpdateFileText(Path.Combine(tmlDevSteamDirectory, "tMLMod.targets"), tMLModTargetsContents);
	}

	private static void UpdateFileText(string path, string text)
	{
		SetupOperation.CreateParentDirectory(path);

		if (!File.Exists(path) || text != File.ReadAllText(path)) {
			File.WriteAllText(path, text);
		}
	}
}