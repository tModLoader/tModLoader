namespace Terraria.ModLoader.Setup.Core;

public sealed class TargetsFilesUpdater
{
	private readonly ProgramSettings programSettings;

	public TargetsFilesUpdater(ProgramSettings programSettings)
	{
		this.programSettings = programSettings;
	}

	public void Update()
	{
		CreateTMLSteamDirIfNecessary();

		UpdateFileText("src/WorkspaceInfo.targets", GetWorkspaceInfoTargetsText(out string branch));
		string tMLModTargetsContents = File.ReadAllText("patches/tModLoader/Terraria/release_extras/tMLMod.targets");

		string? TMLVERSION = Environment.GetEnvironmentVariable("TMLVERSION");
		if (!string.IsNullOrWhiteSpace(TMLVERSION) && branch == "stable") {
			// Convert 2012.4.x to 2012_4
			Console.WriteLine($"TMLVERSION found: {TMLVERSION}");
			string TMLVERSIONDefine = $"TML_{string.Join("_", TMLVERSION.Split('.').Take(2))}";
			Console.WriteLine($"TMLVERSIONDefine: {TMLVERSIONDefine}");
			tMLModTargetsContents = tMLModTargetsContents.Replace("<!-- TML stable version define placeholder -->",
				$"<DefineConstants>$(DefineConstants);{TMLVERSIONDefine}</DefineConstants>");
			UpdateFileText("patches/tModLoader/Terraria/release_extras/tMLMod.targets",
				tMLModTargetsContents); // The patch file needs to be updated as well since it will be copied to src and the postbuild will copy it to the steam folder as well.
		}

		UpdateFileText(Path.Combine(programSettings.TMLDevSteamDir!, "tMLMod.targets"), tMLModTargetsContents);
	}

	private static void UpdateFileText(string path, string text)
	{
		SetupOperation.CreateParentDirectory(path);

		if (!File.Exists(path) || text != File.ReadAllText(path)) {
			File.WriteAllText(path, text);
		}
	}

	private string GetWorkspaceInfoTargetsText(out string branch)
	{
		string gitsha = "";
		RunCmd.Run("", "git", "rev-parse HEAD", s => gitsha = s.Trim());

		string branchResult = "";
		RunCmd.Run("", "git", "rev-parse --abbrev-ref HEAD", s => branchResult = s.Trim());
		branch = branchResult;

		string? GITHUB_HEAD_REF = Environment.GetEnvironmentVariable("GITHUB_HEAD_REF");
		if (!string.IsNullOrWhiteSpace(GITHUB_HEAD_REF)) {
			Console.WriteLine($"GITHUB_HEAD_REF found: {GITHUB_HEAD_REF}");
			branch = GITHUB_HEAD_REF;
		}

		string? HEAD_SHA = Environment.GetEnvironmentVariable("HEAD_SHA");
		if (!string.IsNullOrWhiteSpace(HEAD_SHA)) {
			Console.WriteLine($"HEAD_SHA found: {HEAD_SHA}");
			gitsha = HEAD_SHA;
		}

		return
			$@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <!-- This file will always be overwritten, do not edit it manually. -->
  <PropertyGroup>
	<BranchName>{branch}</BranchName>
	<CommitSHA>{gitsha}</CommitSHA>
	<TerrariaSteamPath>{programSettings.TerrariaSteamDir}</TerrariaSteamPath>
    <tModLoaderSteamPath>{programSettings.TMLDevSteamDir}</tModLoaderSteamPath>
  </PropertyGroup>
</Project>";
	}

	private void CreateTMLSteamDirIfNecessary()
	{
		if (Directory.Exists(programSettings.TMLDevSteamDir))
			return;

		programSettings.TMLDevSteamDir = Path.GetFullPath(Path.Combine(programSettings.TerrariaSteamDir!, "..", "tModLoaderDev"));
		programSettings.Save();

		try {
			Directory.CreateDirectory(programSettings.TMLDevSteamDir);
		}
		catch (Exception e) {
			Console.WriteLine($"{e.GetType().Name}: {e.Message}");
		}
	}
}