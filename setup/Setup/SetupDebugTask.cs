using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Terraria.ModLoader.Setup.Program;

namespace Terraria.ModLoader.Setup
{
	public class SetupDebugTask : Task
	{
		private bool compileFailed;

		public SetupDebugTask(ITaskInterface taskInterface) : base(taskInterface) { }

		public override bool ConfigurationDialog() {
			if (File.Exists(TerrariaPath) && File.Exists(TerrariaServerPath))
				return true;

			return (bool)taskInterface.Invoke(new Func<bool>(SelectTerrariaDialog));
		}

		public override void Run() {
			taskInterface.SetStatus("Copying References");

			var modCompile = Path.Combine(SteamDir, "ModCompile");

			var references = new[] {"FNA.dll", "Mono.Cecil.Pdb.dll", "Mono.Cecil.Mdb.dll" };
			foreach (var dll in references)
				Copy(Path.Combine(baseDir, "references/"+dll), Path.Combine(modCompile, dll));

			var roslynRefs = new[] {"RoslynWrapper.dll", "System.Collections.Immutable.dll", "System.Reflection.Metadata.dll", "Microsoft.CodeAnalysis.dll", "Microsoft.CodeAnalysis.CSharp.dll"};
			foreach (var dll in roslynRefs)
				Copy(Path.Combine(baseDir, "RoslynWrapper/bin/Release/"+dll), Path.Combine(modCompile, dll));

			taskInterface.SetStatus("Updating ModCompile version");
			UpdateModCompileVersion(modCompile);

			taskInterface.SetStatus("Generating launchSettings.json");
			var launchSettingsPath = Path.Combine(baseDir, "src/tModLoader/Properties/launchSettings.json");
			CreateParentDirectory(launchSettingsPath);
			File.WriteAllText(launchSettingsPath, DebugConfig);

			taskInterface.SetStatus("Compiling tModLoaderMac.exe");
			compileFailed = RunCmd(Path.Combine(baseDir, "solutions"), "msbuild",
				"tModLoader.sln /p:Configuration=MacRelease",
				null, null, null, taskInterface.CancellationToken()
			) != 0;
		}

		private void UpdateModCompileVersion(string modCompileDir)
		{
			var modLoaderCsPath = Path.Combine(baseDir, "src", "tModLoader", "Terraria.ModLoader", "ModLoader.cs");
			var r = new Regex(@"new Version\((.+?)\).+?string branchName.+?""(.*?)"".+?int beta.+?(\d+?)", RegexOptions.Singleline);
			var match = r.Match(File.ReadAllText(modLoaderCsPath));

			string version = match.Groups[1].Value.Replace(", ", ".");
			string branchName = match.Groups[2].Value;
			int beta = int.Parse(match.Groups[3].Value);

			var versionTag = $"v{version}" +
				(branchName.Length == 0 ? "" : $"-{branchName.ToLower()}") +
				(beta == 0 ? "" : $"-beta{beta}");

			File.WriteAllText(Path.Combine(modCompileDir, "version"), versionTag);
		}

		private void UpdateModCompileVersion()
		{
			throw new NotImplementedException();
		}

		public override bool Failed() {
			return compileFailed;
		}

		public override void FinishedDialog() {
			MessageBox.Show(
				"Failed to compile tModLoaderMac.exe\r\nJust build it from the tModLoader solution.",
				"Build Failed tModLoaderMac.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private static string DebugConfig => @"{
  ""profiles"": {
    ""Terraria"": {
      ""commandName"": ""Executable"",
      ""executablePath"": ""%steamdir%/tModLoaderDebug.exe"",
      ""workingDirectory"": ""%steamdir%""
    },
    ""TerrariaServer"": {
      ""commandName"": ""Executable"",
      ""executablePath"": ""%steamdir%/tModLoaderServerDebug.exe"",
      ""workingDirectory"": ""%steamdir%""
    }
  }
}".Replace("%steamdir%", SteamDir.Replace('\\', '/'));
	}
}
