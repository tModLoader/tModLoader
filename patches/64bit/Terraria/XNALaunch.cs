#if XNA
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria.Social;
namespace Terraria
{
	public static class WindowsLaunch
	{
		public delegate bool HandlerRoutine(WindowsLaunch.CtrlTypes CtrlType);
		public enum CtrlTypes
		{
			CTRL_C_EVENT,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}
		private static WindowsLaunch.HandlerRoutine _handleRoutine;
		private static bool ConsoleCtrlCheck(WindowsLaunch.CtrlTypes ctrlType)
		{
			bool flag = false;
			switch (ctrlType)
			{
			case WindowsLaunch.CtrlTypes.CTRL_C_EVENT:
				flag = true;
				break;
			case WindowsLaunch.CtrlTypes.CTRL_BREAK_EVENT:
				flag = true;
				break;
			case WindowsLaunch.CtrlTypes.CTRL_CLOSE_EVENT:
				flag = true;
				break;
			case WindowsLaunch.CtrlTypes.CTRL_LOGOFF_EVENT:
			case WindowsLaunch.CtrlTypes.CTRL_SHUTDOWN_EVENT:
				flag = true;
				break;
			}
			if (flag)
			{
				SocialAPI.Shutdown();
			}
			return true;
		}
		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(WindowsLaunch.HandlerRoutine Handler, bool Add);
		[STAThread]
		private static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs sargs)
			{
				string resourceName = new AssemblyName(sargs.Name).Name + ".dll";
				string text = Array.Find<string>(typeof(Program).Assembly.GetManifestResourceNames(), (string element) => element.EndsWith(resourceName));
				if (text == null)
				{
					return null;
				}
				Assembly result;
				using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text))
				{
					byte[] array = new byte[manifestResourceStream.Length];
					manifestResourceStream.Read(array, 0, array.Length);
					result = Assembly.Load(array);
				}
				return result;
			};
#if SERVER
			WindowsLaunch._handleRoutine = new WindowsLaunch.HandlerRoutine(WindowsLaunch.ConsoleCtrlCheck);
			WindowsLaunch.SetConsoleCtrlHandler(WindowsLaunch._handleRoutine, true);
#endif
			Program.LaunchGame(args, false);
		}
	}
}
#endif
