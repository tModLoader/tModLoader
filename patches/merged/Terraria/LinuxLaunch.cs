#if LINUX
using System;

namespace Terraria
{
    internal static class LinuxLaunch
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
            Program.LaunchGame(Utils.FixArgs(args));
        }
    }
}
#endif
