using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using static Terraria.ModLoader.Setup.Settings;

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

            var references = new[] {"FNA.dll", "MP3Sharp.dll"};
            foreach (var dll in references)
                Copy(Path.Combine(Program.baseDir, "references/"+dll), Path.Combine(SteamDir.Get(), dll));

            taskInterface.SetStatus("Generating Debug Configuration");
            File.WriteAllText(Path.Combine(Program.baseDir, "src/tModLoader/Terraria.csproj.user"), DebugConfig);

            taskInterface.SetStatus("Compiling TerrariaMac.exe");
            compileFailed = Program.RunCmd(Path.Combine(Program.baseDir, "solutions"), "msbuild",
                "tModLoader.sln /p:Configuration=MacRelease /p:Platform=\"x86\"",
                null, null, taskInterface.CancellationToken()
            ) != 0;

            if (!compileFailed)
                Copy(Path.Combine(Program.baseDir, @"src\tModLoader\bin\x86\MacRelease\Terraria.exe"), 
                    Path.Combine(SteamDir.Get(), "TerrariaMac.exe"));
        }

        public override bool Failed() {
            return compileFailed;
        }

        public override void FinishedDialog() {
            MessageBox.Show(
                "Failed to compile TerrariaMac.exe\r\nJust build it from the tModLoader solution.",
                "Build Failed TerrariaMac.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static string DebugConfig => @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)' == 'WindowsDebug|x86'"">
    <StartAction>Program</StartAction>
    <StartProgram>%steamdir%\TerrariaDebug.exe</StartProgram>
    <StartWorkingDirectory>%steamdir%</StartWorkingDirectory>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)' == 'WindowsServerDebug|x86'"">
    <StartAction>Program</StartAction>
    <StartProgram>%steamdir%\tModLoaderServerDebug.exe</StartProgram>
    <StartWorkingDirectory>%steamdir%</StartWorkingDirectory>
  </PropertyGroup>
</Project>".Replace("%steamdir%", SteamDir.Get());
    }
}
