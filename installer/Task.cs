using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Installer
{
    abstract class Task
    {
        public void Run(Installer main)
        {
            main.DisableButtons();
            main.SetProgress(0);
            main.SetProgressVisible(true);
            DoTask(main);
            main.SetProgressVisible(false);
            main.SetMessage("");
            main.ReenableButtons();
        }

        public abstract bool DoTask(Installer main);
    }
}
