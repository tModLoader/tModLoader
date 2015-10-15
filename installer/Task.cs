using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Installer
{
    abstract class Task
    {
        protected bool? result;
        internal bool updating;

        public void Run(Installer main)
        {
            main.DisableButtons();
            main.SetProgress(0);
            main.SetProgressVisible(true);

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
            while (!result.HasValue)
            {
                Application.DoEvents();
            }

            main.SetProgressVisible(false);
            main.SetHeader("");
            main.SetMessage("");
            main.ReenableButtons();
        }

        protected abstract bool DoTask(DoWorkArgs args);

        protected void DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = DoTask((DoWorkArgs)e.Argument);
        }

        protected void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is ProgressChangedArgs)
            {
                ProgressChangedArgs args = (ProgressChangedArgs)e.UserState;
                args.main.SetMaxProgress(args.maxProgress);
                if(args.header.Length > 0)
                {
                    args.main.SetHeader(args.header);
                }
                if (args.message.Length > 0)
                {
                    args.main.SetMessage(args.message);
                }
                args.main.SetProgress(e.ProgressPercentage);
            }
            else if (e.UserState is Installer)
            {
                (e.UserState as Installer).IncrementProgress();
            }
            updating = false;
        }

        protected void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            result = (bool)e.Result;
        }

        protected internal void ReportProgress(BackgroundWorker background, int progress, ProgressChangedArgs args)
        {
            updating = true;
            background.ReportProgress(progress, args);
            while (updating) ;
        }

        protected internal void ReportProgress(DoWorkArgs args)
        {
            updating = true;
            args.background.ReportProgress(-1, args.main);
            while (updating) ;
        }
    }
}
