#if !WINDOWS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Terraria;

internal static class MonoLaunch
{
#if NETCORE
	public static readonly object resolverLock = new object();
#endif
	
	private static readonly Dictionary<string, IntPtr> assemblies = new Dictionary<string, IntPtr>();
	
	private static void Main(string[] args) {
#if NETCORE
		AssemblyLoadContext.Default.ResolvingUnmanagedDll += ResolveNativeLibrary;
#endif

		Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
		Program.LaunchGame(args, monoArgs: true);
	}

#if NETCORE
	private static IntPtr ResolveNativeLibrary(Assembly assembly, string name) {
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
				else {
					// Toss an error when failed to load needed library file, instead of just waiting for later to toss - Solxan
					assemblies[name] = IntPtr.Zero;
					throw new FileLoadException("Failed to load Native Library at " + match);
				}
			}
			catch (DirectoryNotFoundException e) {
				throw new DirectoryNotFoundException("A needed library file was missing from the tModLoader directory. " + e.Message, e);
			}
		}
	}

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