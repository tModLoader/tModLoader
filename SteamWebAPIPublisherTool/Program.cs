// See https://aka.ms/new-console-template for more information
using SteamWebAPIPublisherTool;
using SteamWebAPIPublisherTool.SteamCMD;
using SteamWebAPIPublisherTool.SteamWebApi;



// The below code works, but needs two input arguments
SteamWebWrapper.SetPublisherKey("placeholder 1");
SteamCmdDownloaderInstance.SteamCMDPath = "placeholder 2";

string workshopForceDevMetadataFolder = null;
bool performFullRun = string.IsNullOrEmpty(workshopForceDevMetadataFolder);


if (performFullRun) {
	var response = SteamWebWrapper.QueryForPublisherIds();

	for (int i = 0; i < 1/* response.Count*/; i++) {
		string workingDir = BatchMetadataSetRunner.CreateWorkingDirectoryForPage(response[i], i);
		new BatchMetadataSetRunner(workingDir).RunForceDevMetadataUpdate(deleteModsWhenComplete: true);
	}
}
else {
	// Update items only in the associated folder. good for touchups or running in CI
	new BatchMetadataSetRunner(workshopForceDevMetadataFolder).RunForceDevMetadataUpdate(deleteModsWhenComplete: false);
}
	
