using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Terraria.Social.Base;

namespace Terraria.Social.Steam;

internal class BatchMetadataSetRunner
{
	private string workingDirectory;

	internal BatchMetadataSetRunner(string workingDirectory)
	{
		this.workingDirectory = workingDirectory;
	}

	internal static string CreateWorkingDirectoryForPage(string[] publishedFileIds, int currentPage)
	{
		string workingDirectory = GetWorkingDirectory(currentPage);
		Directory.CreateDirectory(workingDirectory);
		File.WriteAllLines($"{workingDirectory}/install.txt", publishedFileIds);

		Console.WriteLine($"Workshop directory for Page #{currentPage} created");
		return workingDirectory;
	}

	private static string GetWorkingDirectory(int pageId) => $"{Directory.GetCurrentDirectory()}/page{pageId}";

	internal void RunForceDevMetadataUpdate(bool deleteModsWhenComplete)
	{
		// Download the Items
		var actualWorkshopItemsFolder = DownloadItemsToFolder();

		// Update the Metadata
		ForceUpdateDevMetadata(actualWorkshopItemsFolder, deleteModsWhenComplete);
	}

	private string DownloadItemsToFolder()
	{
		var downloader = new SteamCmdDownloaderInstance(
			modInstallTxtPath: $"{workingDirectory}/install.txt",
			modDownloadFolderPath: workingDirectory
		);

		return downloader.DownloadItems();
	}

	private void ForceUpdateDevMetadata(string actualWorkshopItemsFolder, bool deleteModsWhenComplete)
	{
		var devMetadataKvp = IterateWorkshopFilesForDevMetadata(actualWorkshopItemsFolder);

		foreach (var item in devMetadataKvp) {
			SteamWebWrapper.SetDeveloperMetadata(item.publishedId, item.metadata);
			Console.WriteLine($"Metadata for Workshop Item {item.publishedId} has been updated");
		}

		// Free up disk drive space by cleaning out workshop items folder when complete
		if (deleteModsWhenComplete)
			Directory.Delete(actualWorkshopItemsFolder, true);
	}

	private List<(string publishedId, string metadata)> IterateWorkshopFilesForDevMetadata(string actualWorkshopItemsFolder)
	{
		// Code to iterate through the .tmod files on workshop; read the hash data and format it for metadata
		var workshopItems = Directory.EnumerateDirectories(actualWorkshopItemsFolder);
		List<(string publishedId, string metadata)> devMetadataKvp = new List<(string publishedId, string metadata)>();

		foreach (var workshopItem in workshopItems) {
			var publishId = Path.GetFileNameWithoutExtension(workshopItem);

			// Read the tmod files in directory & Get metadata
			var devMetadata = new DeveloperMetadata(workshopItem, useWebApi: true);

			devMetadataKvp.Add((publishId, devMetadata.GetSerialize()));
		}

		return devMetadataKvp;
	}
}
