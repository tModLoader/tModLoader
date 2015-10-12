using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Ionic.Zlib;

namespace Installer
{
    class SetupTask : Task
    {
        protected override bool DoTask(DoWorkArgs args)
        {
            ProgressChangedArgs pass = new ProgressChangedArgs();
            pass.maxProgress = 1;
            pass.message = "Finding files...";
            pass.main = args.main;
            int progress = 0;
            ReportProgress(args.background, progress, pass);
            Directory.CreateDirectory("Setup");
            string[] files =
            {
                "Windows.exe",
                "Mac.exe",
                "Linux.exe",
                "Microsoft.Xna.Framework.dll",
                "Microsoft.Xna.Framework.Game.dll",
                "Microsoft.Xna.Framework.Graphics.dll",
                "Microsoft.Xna.Framework.Xact.dll",
                "FNA.dll",
                "MP3Sharp.dll",
                "Content" + Path.DirectorySeparatorChar + "MysteryItem.png"
            };
            string prefix = "Setup" + Path.DirectorySeparatorChar;
            for (int k = 0; k < files.Length; k++)
            {
                files[k] = prefix + files[k];
                if (!File.Exists(files[k]))
                {
                    MessageBox.Show("The file " + files[k] + " does not exist.", "Setup Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            pass.maxProgress = 2 * files.Length + 1;
            Directory.CreateDirectory("Resources");
            try
            {
                ZipFile zip = new ZipFile("Resources" + Path.DirectorySeparatorChar + "Install");
                foreach(string file in files)
                {
                    string fileName = file.Substring(prefix.Length + 1);
                    pass.message = "Packing " + fileName + "...";
                    progress++;
                    ReportProgress(args.background, progress, pass);
                    zip[fileName] = File.ReadAllBytes(file);
                }
                pass.message = "Saving installation resources...";
                progress++;
                ReportProgress(args.background, progress, pass);
                zip.Write(this, args);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Setup Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            MessageBox.Show("Success!", "Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
    }
}
