// See https://aka.ms/new-console-template for more information
using SteamWebAPIPublisherTool;
using SteamWebAPIPublisherTool.SteamWebApi;


SteamWebWrapper.SetPublisherKey("placeholder");
var response = SteamWebWrapper.QueryForPublisherIds();

var runner = new BatchMetadataSetRunner(response[0], 0);
runner.RunForceDevMetadataUpdate();

var a = 1;
