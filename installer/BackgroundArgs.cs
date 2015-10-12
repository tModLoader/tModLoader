using System;
using System.ComponentModel;

namespace Installer
{
    struct DoWorkArgs
    {
        public BackgroundWorker background;
        public Installer main;
    }

    struct ProgressChangedArgs
    {
        public int maxProgress;
        public string message;
        public Installer main;
    }
}