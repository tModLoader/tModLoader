#if LINUX
using System;
using System.IO;
using System.Reflection;

namespace Terraria
{
	internal static class LinuxLaunch
	{
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
			Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
			Program.LaunchGame(Utils.FixArgs(args));
		}
	}
}
#endif
