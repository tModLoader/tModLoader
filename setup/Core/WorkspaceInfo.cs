using System.Xml.Linq;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.Core;

public sealed class WorkspaceInfo
{
	private const string DirectoryName = "src";
	private const string FileName = "WorkspaceInfo.targets";

	private const string InitialFile = """
	                                   <?xml version="1.0" encoding="utf-8"?>
	                                   <Project ToolsVersion="14.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
	                                     <!-- This file will always be overwritten, do not edit it manually. -->
	                                     <PropertyGroup>
	                                       <BranchName></BranchName>
	                                       <CommitSHA></CommitSHA>
	                                       <TerrariaSteamPath></TerrariaSteamPath>
	                                       <tModLoaderSteamPath></tModLoaderSteamPath>
	                                     </PropertyGroup>
	                                   </Project>
	                                   """;

	private const string Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

	private readonly XDocument document;
	private readonly XElement branchNameNode;
	private readonly XElement commitHashNode;
	private readonly XElement terrariaSteamDirectoryNode;
	private readonly XElement tmlDevSteamDirectoryNode;
	private readonly List<Action<string>> tmlDevSteamDirectoryChangedListeners = [];

	private WorkspaceInfo()
	{
		document = XDocument.Load(FilePath);
		branchNameNode = document.Descendants(XName.Get("BranchName", Namespace)).First();
		commitHashNode = document.Descendants(XName.Get("CommitSHA", Namespace)).First();
		terrariaSteamDirectoryNode = document.Descendants(XName.Get("TerrariaSteamPath", Namespace)).First();
		tmlDevSteamDirectoryNode = document.Descendants(XName.Get("tModLoaderSteamPath", Namespace)).First();
	}

	private static string FilePath => Path.Combine(DirectoryName, FileName);

	public string BranchName {
		get => branchNameNode.Value;
		private set => branchNameNode.Value = value;
	}

	public string CommitHash {
		get => commitHashNode.Value;
		private set => commitHashNode.Value = value;
	}

	public string TerrariaSteamDirectory {
		get => terrariaSteamDirectoryNode.Value;
		private set => terrariaSteamDirectoryNode.Value = value;
	}

	public string TMLDevSteamDirectory {
		get => tmlDevSteamDirectoryNode.Value;
		private set {
			tmlDevSteamDirectoryNode.Value = value;

			foreach (Action<string> listener in tmlDevSteamDirectoryChangedListeners) {
				listener.Invoke(value);
			}
		}
	}

	public string TerrariaPath => Path.Combine(TerrariaSteamDirectory, "Terraria.exe");

	public string TerrariaServerPath => Path.Combine(TerrariaSteamDirectory, "TerrariaServer.exe");

	public static WorkspaceInfo Initialize()
	{
		if (!File.Exists(FilePath)) {
			Directory.CreateDirectory(DirectoryName);
			File.WriteAllText(FilePath, InitialFile);
		}

		WorkspaceInfo result = new();
		result.UpdateGitInfo();

		return result;
	}

	public void UpdateGitInfo()
	{
		string? githubHeadRef = Environment.GetEnvironmentVariable("GITHUB_HEAD_REF");
		if (!string.IsNullOrWhiteSpace(githubHeadRef)) {
			Console.WriteLine($"GITHUB_HEAD_REF found: {githubHeadRef}");
			BranchName = githubHeadRef;
		}
		else {
			RunCmd.Run("", "git", "rev-parse --abbrev-ref HEAD", s => BranchName = s.Trim());
		}

		string? headCommitHash = Environment.GetEnvironmentVariable("HEAD_SHA");
		if (!string.IsNullOrWhiteSpace(headCommitHash)) {
			Console.WriteLine($"HEAD_SHA found: {headCommitHash}");
			CommitHash = headCommitHash;
		}
		else {
			RunCmd.Run("", "git", "rev-parse HEAD", s => CommitHash = s.Trim());
		}

		WriteToFile();
	}

	public void UpdatePaths(string terrariaSteamDirectory, string? tMLDevSteamDirectory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(terrariaSteamDirectory);

		TerrariaSteamDirectory = PathUtils.GetCrossPlatformFullPath(terrariaSteamDirectory);
		TMLDevSteamDirectory = PathUtils.GetCrossPlatformFullPath(
			string.IsNullOrWhiteSpace(tMLDevSteamDirectory)
				? Path.Combine(terrariaSteamDirectory, "..", "tModLoaderDev")
				: tMLDevSteamDirectory);

		Directory.CreateDirectory(TMLDevSteamDirectory);

		WriteToFile();
	}

	public void OnTmlDevSteamDirectoryChanged(Action<string> listener)
	{
		tmlDevSteamDirectoryChangedListeners.Add(listener);
	}

	private void WriteToFile()
	{
		document.Save(FilePath);
	}
}