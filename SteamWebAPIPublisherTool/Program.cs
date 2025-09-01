// See https://aka.ms/new-console-template for more information
using SteamWebAPIPublisherTool;
using SteamWebAPIPublisherTool.SteamCMD;
using SteamWebAPIPublisherTool.SteamWebApi;



// The below code works, but needs two input arguments
SteamWebWrapper.SetPublisherKey("placeholder");
SteamCmdDownloaderInstance.SetSteamCmdPath("placeholder 2");

var response = SteamWebWrapper.QueryForPublisherIds();

for (int i = 0; i < response.Count; i++) {
	string workingDir = BatchMetadataSetRunner.CreateWorkingDirectoryForPage(response[i], i);
	new BatchMetadataSetRunner(workingDir).RunForceDevMetadataUpdate();
}
