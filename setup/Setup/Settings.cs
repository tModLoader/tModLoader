using System;
using System.IO;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
    public static class Settings
    {
        public static ProgramSetting<bool> SingleDecompileThread = new ProgramSetting<bool>("SingleDecompileThread");
        public static ProgramSetting<string> SteamDir = new ProgramSetting<string>("SteamDir");

        public static string TerrariaPath { get { return Path.Combine(SteamDir.Get(), "Terraria.exe"); } }
        public static string TerrariaServerPath { get { return Path.Combine(SteamDir.Get(), "TerrariaServer.exe"); } }

        public static ProgramSetting<DateTime> MergedDiffCutoff = new ProgramSetting<DateTime>("MergedDiffCutoff");
        public static ProgramSetting<DateTime> TerrariaDiffCutoff = new ProgramSetting<DateTime>("TerrariaDiffCutoff");
        public static ProgramSetting<DateTime> tModLoaderDiffCutoff = new ProgramSetting<DateTime>("tModLoaderDiffCutoff");

        public static bool SelectTerrariaDialog() {
            while (true) {
                var dialog = new OpenFileDialog {
                    InitialDirectory = Path.GetFullPath(Directory.Exists(SteamDir.Get()) ? SteamDir.Get() : Program.baseDir),
                    Filter = "Terraria|Terraria.exe",
                    Title = "Select Terraria.exe"
                };

                if (dialog.ShowDialog() != DialogResult.OK)
                    return false;

                string err = null;
                if (Path.GetFileName(dialog.FileName) != "Terraria.exe")
                    err = "File must be named Terraria.exe";
                else if (!File.Exists(Path.Combine(Path.GetDirectoryName(dialog.FileName), "TerrariaServer.exe")))
                    err = "TerrariaServer.exe does not exist in the same directory";

                if (err != null) {
                    if (MessageBox.Show(err, "Invalid Selection", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                        return false;
                }
                else {
                    SteamDir.Set(Path.GetDirectoryName(dialog.FileName));
                    return true;
                }
            }
        }
    }
}
