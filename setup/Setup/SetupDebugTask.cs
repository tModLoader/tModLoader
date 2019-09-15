using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Terraria.ModLoader.Setup.Program;

namespace Terraria.ModLoader.Setup
{
	public class SetupDebugTask : Task
	{
		private bool roslynCompileFailed;
		private bool tMLFNACompileFailed;

		public SetupDebugTask(ITaskInterface taskInterface) : base(taskInterface) { }

		public override bool ConfigurationDialog() {
			if (File.Exists(TerrariaPath) && File.Exists(TerrariaServerPath))
				return true;

			return (bool)taskInterface.Invoke(new Func<bool>(SelectTerrariaDialog));
		}

		public override void Run() {
			taskInterface.SetStatus("Copying References");

			taskInterface.SetStatus("Generating launchSettings.json");
			var launchSettingsPath = Path.Combine(baseDir, "src/tModLoader/Properties/launchSettings.json");
			CreateParentDirectory(launchSettingsPath);
			File.WriteAllText(launchSettingsPath, DebugConfig);

			taskInterface.SetStatus("Updating ModCompile version");
			var modCompile = Path.Combine(SteamDir, "ModCompile");
			UpdateModCompileVersion(modCompile);

			var references = new[] { "FNA.dll" };
			foreach (var dll in references)
				Copy(Path.Combine(baseDir, "references/"+dll), Path.Combine(modCompile, dll));

			bool msBuildOnPath = RunCmd(Path.Combine(baseDir, "RoslynWrapper"), "where",
				"msbuild",
				(s) => Console.WriteLine(s), null, null, taskInterface.CancellationToken()
			) == 0;
			if (!msBuildOnPath)
				throw new Exception("msbuild not found on PATH");

			roslynCompileFailed = RunCmd(Path.Combine(baseDir, "RoslynWrapper"), "msbuild",
				"RoslynWrapper.sln /restore /p:Configuration=Release",
				null, null, null, taskInterface.CancellationToken()
			) != 0;

			var roslynRefs = new[] {"RoslynWrapper.dll", "Microsoft.CodeAnalysis.dll", "Microsoft.CodeAnalysis.CSharp.dll", 
				"System.Collections.Immutable.dll", "System.Reflection.Metadata.dll", "System.IO.FileSystem.dll", "System.IO.FileSystem.Primitives.dll",
				"System.Security.Cryptography.Algorithms.dll", "System.Security.Cryptography.Encoding.dll", "System.Security.Cryptography.Primitives.dll", "System.Security.Cryptography.X509Certificates.dll" };
			foreach (var dll in roslynRefs)
				Copy(Path.Combine(baseDir, "RoslynWrapper/bin/Release/net46", dll), Path.Combine(modCompile, dll));

			taskInterface.SetStatus("Compiling tModLoader.FNA.exe");
			tMLFNACompileFailed = RunCmd(Path.Combine(baseDir, "solutions"), "msbuild",
				"tModLoader.sln /restore /p:Configuration=MacRelease",
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

			Directory.CreateDirectory(modCompileDir);
			File.WriteAllText(Path.Combine(modCompileDir, "version"), versionTag);
		}

		public override bool Failed() {
			return roslynCompileFailed || tMLFNACompileFailed;
		}

		public override void FinishedDialog() {
			if (roslynCompileFailed)
				MessageBox.Show("MSBuild Error", "Failed to compile RoslynWrapper.sln.", MessageBoxButtons.OK, MessageBoxIcon.Error);

			if (tMLFNACompileFailed)
				MessageBox.Show(
					"Failed to compile tModLoader.FNA.exe\r\nJust build it from the tModLoader solution.",
					"MSBuild Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private static string DebugConfig => @"{
  ""profiles"": {
    ""Terraria"": {
      ""commandName"": ""Executable"",
      ""executablePath"": ""%steamdir%/$(OutputName).exe"",
      ""workingDirectory"": ""%steamdir%""
    }
  }
}".Replace("%steamdir%", SteamDir.Replace('\\', '/'));
	}
}
