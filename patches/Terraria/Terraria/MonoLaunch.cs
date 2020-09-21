#if !WINDOWS
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
#if NETCORE
		NativeLibrary.SetDllImportResolver(typeof(Game).Assembly, resolver);
		NativeLibrary.SetDllImportResolver(typeof(Steamworks.SteamAPI).Assembly, resolver);
#endif
		Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
		Program.LaunchGame(args, monoArgs: true);
	}

#if NETCORE
	private static IntPtr resolver(string name, Assembly assembly, DllImportSearchPath? searchPath) {
		try {
			if (assemblies.TryGetValue(name, out var handle)) {
				return handle;
			}
			var path = Path.Combine(Environment.CurrentDirectory, "Libraries", "Native", getNativeDir(name));
			var files = Directory.GetFiles(path, name + "*", SearchOption.AllDirectories);
			var match = files.FirstOrDefault(s => File.Exists(s));
			if (match != null && NativeLibrary.TryLoad(match, out handle)) {
				return assemblies[name] = handle;
			}
			return assemblies[name] = IntPtr.Zero;
		}
		catch (DirectoryNotFoundException e) {
			throw new DirectoryNotFoundException("A needed library file was missing from the tModLoader directory. " + e.Message, e);
		}
	}
	private static readonly Dictionary<string, IntPtr> assemblies = new Dictionary<string, IntPtr>();

	private static string getNativeDir(string name) {
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return "Windows";
		if (name.Contains("steam", StringComparison.OrdinalIgnoreCase))
			return "Linux";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return "Linux";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return "OSX";
		throw new InvalidOperationException("Unknown OS.");
	}
#endif
}
#endif