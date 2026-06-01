using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;
using Terraria.Social.Base;

namespace Terraria.Social.Steam;

internal class BatchMetadataSetRunner
{
	private string workingDirectory;

	internal static void RunForceUpdate(string workshopForceDevMetadataFolder = null)
	{
		SteamCmdDownloaderInstance.SteamCMDPath = Environment.GetEnvironmentVariable("steamcmd_path");
		SteamCmdDownloaderInstance.SteamCMDUser = Environment.GetEnvironmentVariable("steamcmd_user");

		bool performFullRun = string.IsNullOrEmpty(workshopForceDevMetadataFolder);


		if (performFullRun) {
			var response = SteamWebWrapper.QueryForPublisherIds();

			for (int i = 0; i < response.Count; i++) {
				try {
					string workingDir = CreateWorkingDirectoryForPage(response[i], i);
					new BatchMetadataSetRunner(workingDir).RunForceDevMetadataUpdate(deleteModsWhenComplete: true);

				}
				catch (Exception e) {
					Logging.tML.Warn($"Page {i} failed to complete;\n{e}");
				}
			}
		}
		else {
			// Update items only in the associated folder. good for touchups or running in CI
			new BatchMetadataSetRunner(workshopForceDevMetadataFolder).RunForceDevMetadataUpdate(deleteModsWhenComplete: false);
		}

		Environment.Exit(0);
	}

	private BatchMetadataSetRunner(string workingDirectory)
	{
		this.workingDirectory = workingDirectory;
	}

	internal static string CreateWorkingDirectoryForPage(string[] publishedFileIds, int currentPage)
	{
		string workingDirectory = GetWorkingDirectory(currentPage);
		Directory.CreateDirectory(workingDirectory);
		File.WriteAllLines($"{workingDirectory}/install.txt", publishedFileIds);

		Logging.tML.Info($"Workshop directory for Page #{currentPage} created");
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
			//SteamWebWrapper.SetKeyValueTags(publishedFileId, keyValueTags);

			Logging.tML.Info($"Metadata for Workshop Item {item.publishedId} has been updated");
		}

		// Free up disk drive space by cleaning out workshop items folder when complete
		if (deleteModsWhenComplete)
			Directory.Delete(Path.Combine(workingDirectory, "steamapps"), true);
	}

	private List<(string publishedId, string metadata)> IterateWorkshopFilesForDevMetadata(string actualWorkshopItemsFolder)
	{
		// Code to iterate through the .tmod files on workshop; read the hash data and format it for metadata
		var workshopItems = Directory.EnumerateDirectories(actualWorkshopItemsFolder);
		List<(string publishedId, string metadata)> devMetadataKvp = new List<(string publishedId, string metadata)>();

		foreach (var workshopItem in workshopItems) {
			var publishId = Path.GetFileNameWithoutExtension(workshopItem);

			// Read the tmod files in directory & Get metadata
			var devMetadata = WorkshopSocialModule.GetDeveloperMetadataForPublish(workshopItem, ulong.Parse(publishId));

			devMetadataKvp.Add((publishId, devMetadata.Serialize()));
		}

		return devMetadataKvp;
	}
}
