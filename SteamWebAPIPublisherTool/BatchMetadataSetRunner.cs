using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SteamWebAPIPublisherTool;
internal class BatchMetadataSetRunner
{
	internal const string SteamCMDPath = "placeholder";

	private string[] publishedFileIds;
	private int pageId;

	internal BatchMetadataSetRunner(string[] publishedFileIds, int currentPage)
	{
		this.publishedFileIds = publishedFileIds;
		this.pageId = currentPage;
	}

	internal string WorkingDirectory => $"{Directory.GetCurrentDirectory()}/page{pageId}"; 

	internal async void RunForceDevMetadataUpdate()
	{
		// Prepare the Directory
		Directory.CreateDirectory(WorkingDirectory);
		File.WriteAllLines($"{WorkingDirectory}/install.txt", publishedFileIds);

		// Download the Items
		var actualWorkshopItemsFolder = DownloadItemsToFolder();

		// Update the Metadata
		ForceUpdateDevMetadata(actualWorkshopItemsFolder);
	}

	private string DownloadItemsToFolder()
	{
		var downloader = new SteamCMD.SteamCmdDownloaderInstance(
			steamCmdExePath: SteamCMDPath,
			modInstallTxtPath: $"{WorkingDirectory}/install.txt",
			modDownloadFolderPath: WorkingDirectory
		);

		return downloader.DownloadItems();
	}

	private void ForceUpdateDevMetadata(string actualWorkshopItemsFolder)
	{
		var devMetadataKvp = IterateWorkshopFilesForDevMetadata(actualWorkshopItemsFolder);

		foreach (var item in devMetadataKvp) {
			SteamWebApi.SteamWebWrapper.SetDeveloperMetadata(item.publishedId, item.metadata);
		}
	}

	private List<(string publishedId, string metadata)> IterateWorkshopFilesForDevMetadata(string actualWorkshopItemsFolder)
	{
		// Code to iterate through the .tmod files on workshop; read the hash data and format it for metadata
		var workshopItems = Directory.EnumerateDirectories(actualWorkshopItemsFolder);
		List<(string publishedId, string metadata)> devMetadataKvp = new List<(string publishedId, string metadata)>();

		foreach (var workshopItem in workshopItems) {
			var publishId = Path.GetFileNameWithoutExtension(workshopItem);

			// Read the tmod files in directory & Get metadata
			string devMetadata = CalculateDevMetadata(workshopItem);

			devMetadataKvp.Add((publishId, devMetadata));
		}

		return devMetadataKvp;
	}

	private string CalculateDevMetadata(string workshopItemFolder)
	{
		return "Placeholder";
	}
}
