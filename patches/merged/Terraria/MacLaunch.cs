#if MAC
using System;

namespace Terraria
{
    internal static class MacLaunch
    {
        private static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
			Terraria.Main.OnEnginePreload += delegate()
			{
				Terraria.Main.instance.IsMouseVisible = false;
			};
            Program.LaunchGame(Utils.FixArgs(args));
        }
    }
}
#endif
