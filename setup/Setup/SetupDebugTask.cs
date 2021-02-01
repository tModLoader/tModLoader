using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Terraria.ModLoader.Setup.Program;

namespace Terraria.ModLoader.Setup
{
	public class SetupDebugTask : SetupOperation
	{
		private bool roslynCompileFailed;
		private bool tMLFNACompileFailed;

		public SetupDebugTask(ITaskInterface taskInterface) : base(taskInterface) { }

		public override void Run() {
			taskInterface.SetStatus("Updating ModCompile version");
			var modCompile = Path.Combine(tMLSteamDir, "ModCompile");
			UpdateModCompileVersion(modCompile);

			taskInterface.SetStatus("Compiling RoslynWrapper");

			//Compile Roslyn.
			roslynCompileFailed = RunCmd("RoslynWrapper", "dotnet", "build", cancel: taskInterface.CancellationToken) != 0;

			//Try to install Roslyn's libraries.

			string[] roslynRefs = new[] {"RoslynWrapper.dll", "Microsoft.CodeAnalysis.dll", "Microsoft.CodeAnalysis.CSharp.dll",
				"System.Collections.Immutable.dll", "System.Reflection.Metadata.dll", "System.IO.FileSystem.dll", "System.IO.FileSystem.Primitives.dll",
				"System.Security.Cryptography.Algorithms.dll", "System.Security.Cryptography.Encoding.dll", "System.Security.Cryptography.Primitives.dll", "System.Security.Cryptography.X509Certificates.dll" };

			foreach (string dll in roslynRefs) {
				string path = Path.Combine("RoslynWrapper/bin/Release/net46", dll);

				if (!roslynCompileFailed || File.Exists(path))
					Copy(path, Path.Combine(modCompile, dll));
			}

			//Compile FNA tML.
			taskInterface.SetStatus("Compiling tModLoader.FNA.exe");
			tMLFNACompileFailed = RunCmd("src/tModLoader/Terraria", "dotnet", "build -c MacRelease", cancel: taskInterface.CancellationToken) != 0;
		}

		private void UpdateModCompileVersion(string modCompileDir) {
			var modLoaderCsPath = Path.Combine("src", "tModLoader", "Terraria", "Terraria.csproj");
			var r = new Regex(@"<tMLVersion>(.+?)</tMLVersion>", RegexOptions.Singleline);
			var match = r.Match(File.ReadAllText(modLoaderCsPath));

			// ModCompile.GetModCompileVersion currently does not care about other fields, like branch, purpose, BuildDate, sha. It probably should.
			string version = match.Groups[1].Value;
			var versionTag = $"v{version}";
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
	}
}
