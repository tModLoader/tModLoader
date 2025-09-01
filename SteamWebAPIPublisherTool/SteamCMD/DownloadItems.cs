using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWebAPIPublisherTool.SteamCMD;

internal class SteamCmdDownloaderInstance
{
	private static string SteamCMDPath = null;
	private string modInstallTxtPath;
	private string modDownloadFolderPath;

	internal SteamCmdDownloaderInstance(string modInstallTxtPath, string modDownloadFolderPath)
	{
		this.modInstallTxtPath = modInstallTxtPath;
		this.modDownloadFolderPath = modDownloadFolderPath;
	}

	//TODO: Replace this with internal Set; Get;
	internal static void SetSteamCmdPath(string path)
	{
		SteamCMDPath = path;
	}

	private string SteamCmdLeadingArguments(string steamCmdDownloadList) =>
		$"+force_install_dir {modDownloadFolderPath} +login anonymous {steamCmdDownloadList} +quit";

	private string GetActualModDownloadsWorkshopFolder() =>
		$"{modDownloadFolderPath}/steamapps/workshop/content/1281930";

	/// <summary>
	/// 
	/// </summary>
	/// <returns>The Actual Workshop Folder that the mods were downloaded to</returns>
	internal string DownloadItems() {
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

		Console.WriteLine($"Starting SteamCmd Workshop Download Items...");

		var downloader = Process.Start(steamCmdStartInfo);
		downloader.WaitForExit();

		Console.WriteLine($"SteamCmd Workshop Download Items completed.");

		return GetActualModDownloadsWorkshopFolder();
	}
}
