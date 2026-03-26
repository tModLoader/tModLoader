using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria.Social.Steam;

internal class SteamCmdDownloaderInstance
{
	internal static string SteamCMDPath { get; set; } = null;
	internal static string SteamCMDUser { get; set; } = "anonymous";

	private string modInstallTxtPath;
	private string modDownloadFolderPath;

	internal SteamCmdDownloaderInstance(string modInstallTxtPath, string modDownloadFolderPath)
	{
		this.modInstallTxtPath = modInstallTxtPath;
		this.modDownloadFolderPath = modDownloadFolderPath;
	}

	private string SteamCmdLeadingArguments(string steamCmdDownloadList) =>
		$"+force_install_dir \"{modDownloadFolderPath}\" +login {SteamCMDUser} {steamCmdDownloadList} +quit";

	private string GetActualModDownloadsWorkshopFolder() =>
		$"{modDownloadFolderPath}/steamapps/workshop/content/1281930";

	/// <summary>
	///
	/// </summary>
	/// <returns>The Actual Workshop Folder that the mods were downloaded to</returns>
	internal string DownloadItems()
	{
		if (SteamCMDPath is null)
			throw new Exception("SteamCMD Path must be set prior to attempting to download items!");

		// Read Install.txt file relevant to this instance
		var publishIds = File.ReadAllLines(modInstallTxtPath);
		var publishIdsArgument = string.Join("", publishIds.Select(id => $" +workshop_download_item 1281930 {id}"));

		// Run SteamCMD
		ProcessStartInfo steamCmdStartInfo = new ProcessStartInfo() {
			Arguments = SteamCmdLeadingArguments(publishIdsArgument),
			UseShellExecute = true,
			FileName = SteamCMDPath
		};

		Logging.tML.Info($"Starting SteamCmd Workshop Download Items...");

		var downloader = Process.Start(steamCmdStartInfo);
		downloader.WaitForExit();

		// Check if all items were downloaded and log when it wasn't
		var workshopFolder = GetActualModDownloadsWorkshopFolder();
		foreach (var modPath in Directory.GetDirectories(workshopFolder)) {
			var modId = Path.GetFileNameWithoutExtension(modPath);

			if (!publishIds.Contains(modId))
				Logging.tML.Warn($"PublishID {modId} Failed to Download. Skipping metadata edits");
		}

		Logging.tML.Info($"SteamCmd Workshop Download Items completed for {modDownloadFolderPath}.");

		return workshopFolder;
	}
}
