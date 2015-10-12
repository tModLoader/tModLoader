using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Ionic.Zlib;

namespace Installer
{
    class SetupTask : Task
    {
        private bool? result;
        private bool updating;

        public override bool DoTask(Installer main)
        {
            result = null;
            BackgroundWorker background = new BackgroundWorker();
            background.WorkerReportsProgress = true;
            background.DoWork += DoWork;
            background.ProgressChanged += ProgressChanged;
            background.RunWorkerCompleted += RunWorkerCompleted;
            DoWorkArgs args = new DoWorkArgs();
            args.background = background;
            args.main = main;
            background.RunWorkerAsync(args);
            while (background.IsBusy)
            {
                Application.DoEvents();
            }
            return result.Value;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Setup((DoWorkArgs)e.Argument);
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressChangedArgs args = (ProgressChangedArgs)e.UserState;
            args.main.SetMaxProgress(args.maxProgress);
            args.main.SetMessage(args.message);
            args.main.SetProgress(e.ProgressPercentage);
            updating = false;
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            result = (bool)e.Result;
        }

        private bool Setup(DoWorkArgs args)
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
            pass.maxProgress = files.Length + 2;
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
                zip.Write();
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

        private void ReportProgress(BackgroundWorker background, int progress, ProgressChangedArgs args)
        {
            updating = true;
            background.ReportProgress(progress, args);
            while (updating) ;
        }

        private struct DoWorkArgs
        {
            public BackgroundWorker background;
            public Installer main;
        }

        private struct ProgressChangedArgs
        {
            public int maxProgress;
            public string message;
            public Installer main;
        }
    }
}
