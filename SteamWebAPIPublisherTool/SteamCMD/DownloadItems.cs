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
	private string steamCmdExePath;
	private string modInstallTxtPath;
	private string modDownloadFolderPath;

	internal SteamCmdDownloaderInstance(string steamCmdExePath, string modInstallTxtPath, string modDownloadFolderPath)
	{
		this.steamCmdExePath = steamCmdExePath;
		this.modInstallTxtPath = modInstallTxtPath;
		this.modDownloadFolderPath = modDownloadFolderPath;
	}

	private string SteamCmdLeadingArguments(string steamCmdDownloadList) =>
		$"+force_install_dir {modDownloadFolderPath} +login anonymous {steamCmdDownloadList}";

	private string GetActualModDownloadsWorkshopFolder() =>
		$"{modDownloadFolderPath}/steamapps/workshop/content/1281930";

	/// <summary>
	/// 
	/// </summary>
	/// <returns>The Actual Workshop Folder that the mods were downloaded to</returns>
	internal string DownloadItems() {
		// Read Install.txt file relevant to this instance
		var publishIds = File.ReadAllLines(modInstallTxtPath);
		var publishIdsArgument = string.Join("", publishIds.Select(id => $" +workshop_download_item 1281930 {id}"));

		// Run SteamCMD
		ProcessStartInfo steamCmdStartInfo = new ProcessStartInfo() {
			Arguments = SteamCmdLeadingArguments(publishIdsArgument),
			UseShellExecute = true,
			FileName = steamCmdExePath
		};

		Process.Start(steamCmdStartInfo);

		return GetActualModDownloadsWorkshopFolder();
	}
}
