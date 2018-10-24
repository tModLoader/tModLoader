using System;
using System.IO;
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

			var references = new[] {"FNA.dll", "Mono.Cecil.Pdb.dll" };
			foreach (var dll in references)
				Copy(Path.Combine(baseDir, "references/"+dll), Path.Combine(modCompile, dll));

			var roslynRefs = new[] {"RoslynWrapper.dll", "System.Collections.Immutable.dll", "System.Reflection.Metadata.dll", "Microsoft.CodeAnalysis.dll", "Microsoft.CodeAnalysis.CSharp.dll"};
			foreach (var dll in roslynRefs)
				Copy(Path.Combine(baseDir, "RoslynWrapper/bin/Release/"+dll), Path.Combine(modCompile, dll));

			taskInterface.SetStatus("Generating Debug Configuration");
			File.WriteAllText(Path.Combine(baseDir, "src/tModLoader/Terraria.csproj.user"), DebugConfig);

			taskInterface.SetStatus("Compiling tModLoaderMac.exe");
			compileFailed = RunCmd(Path.Combine(baseDir, "solutions"), "msbuild",
				"tModLoader.sln /p:Configuration=MacRelease /p:Platform=\"x86\"",
				null, null, null, taskInterface.CancellationToken()
			) != 0;
		}

		public override bool Failed() {
			return compileFailed;
		}

		public override void FinishedDialog() {
			MessageBox.Show(
				"Failed to compile tModLoaderMac.exe\r\nJust build it from the tModLoader solution.",
				"Build Failed tModLoaderMac.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private static string DebugConfig => @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)' == 'WindowsDebug|x86'"">
    <StartAction>Program</StartAction>
    <StartProgram>%steamdir%\tModLoaderDebug.exe</StartProgram>
    <StartWorkingDirectory>%steamdir%</StartWorkingDirectory>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)' == 'WindowsServerDebug|x86'"">
    <StartAction>Program</StartAction>
    <StartProgram>%steamdir%\tModLoaderServerDebug.exe</StartProgram>
    <StartWorkingDirectory>%steamdir%</StartWorkingDirectory>
  </PropertyGroup>
</Project>".Replace("%steamdir%", SteamDir);
	}
}
