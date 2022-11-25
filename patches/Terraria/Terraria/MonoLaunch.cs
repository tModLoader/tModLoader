#if !WINDOWS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;

namespace Terraria;

internal static class MonoLaunch
{
	private static readonly Dictionary<string, IntPtr> assemblies = new Dictionary<string, IntPtr>();

	private static void Main(string[] args)
	{
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