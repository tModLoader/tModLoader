#if !WINDOWS
using System;
using System.IO;
using System.Reflection;
using Terraria;

internal static class MonoLaunch
{
	private static void Main(string[] args) {
		AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs sargs) {
			string resourceName = new AssemblyName(sargs.Name).Name + ".dll";
			string text = Array.Find(typeof(Program).Assembly.GetManifestResourceNames(), (string element) => element.EndsWith(resourceName));
			if (text == null)
				return null;

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text)) {
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				return Assembly.Load(array);
			}
		};

		Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
		Program.LaunchGame(args, monoArgs: true);
	}
}
#endif