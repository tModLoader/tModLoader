#if !WINDOWS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria;

internal static class MonoLaunch
{
#if NETCORE
	public static readonly object resolverLock = new object();
#endif
	
	private static void Main(string[] args) {
		// FNA is requested by both Terraria and ReLogic.dll
		var loaded = new Dictionary<string, Assembly>();

		AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs sargs) {
			string resourceName = new AssemblyName(sargs.Name).Name + ".dll";
			string text = Array.Find(typeof(Program).Assembly.GetManifestResourceNames(), (string element) => element.EndsWith(resourceName));
			if (text == null)
				return null;

			if (loaded.TryGetValue(text, out var assembly))
				return assembly;

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text)) {
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				loaded[text] = assembly = Assembly.Load(array);
#if NETCORE
				NativeLibrary.SetDllImportResolver(assembly, ResolveNativeLibrary);
#endif
				return assembly;
			}
		};
		Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
		Program.LaunchGame(args, monoArgs: true);
	}

#if NETCORE
	private static IntPtr ResolveNativeLibrary(string name, Assembly assembly, DllImportSearchPath? searchPath) {
		lock (resolverLock) {
			try {
				if (assemblies.TryGetValue(name, out var handle)) {
					return handle;
				}
				
				Console.WriteLine($"Native Resolve: {assembly.FullName} -> {name}");

				var dir = Path.Combine(Environment.CurrentDirectory, "Libraries", "Native", getNativeDir(name));
				var files = Directory.GetFiles(dir, $"*{name}*", SearchOption.AllDirectories);
				var match = files.FirstOrDefault();
				
				Console.WriteLine(match == null ? "\tnot found in Libraries/Native" : $"\tattempting load {match}");
				
				if (match != null && NativeLibrary.TryLoad(match, out handle)) {
					Console.WriteLine("\tsuccess");
					return assemblies[name] = handle;
				}
				
				return assemblies[name] = IntPtr.Zero;
			}
			catch (DirectoryNotFoundException e) {
				throw new DirectoryNotFoundException("A needed library file was missing from the tModLoader directory. " + e.Message, e);
			}
		}
	}
	private static readonly Dictionary<string, IntPtr> assemblies = new Dictionary<string, IntPtr>();

	private static string getNativeDir(string name) {
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return "Windows";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return "Linux";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return "OSX";
		throw new InvalidOperationException("Unknown OS.");
	}
#endif
}
#endif